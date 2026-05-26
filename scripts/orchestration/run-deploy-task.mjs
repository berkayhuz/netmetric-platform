import { existsSync, readdirSync } from "node:fs";
import path from "node:path";
import { spawnSync } from "node:child_process";

const task = process.argv[2];
const rootDir = process.cwd();
const deployDir = path.join(rootDir, "deploy");
const domainLabel = "deploy";

const statusInfo = (status, message) => {
  console.log(`[${domainLabel}] STATUS=${status} ${message}`);
};

const statusError = (status, message) => {
  console.error(`[${domainLabel}] STATUS=${status} ${message}`);
};

const commandAvailable = (command) => {
  const check = spawnSync(command, ["--version"], {
    stdio: "pipe",
    encoding: "utf8",
  });
  return check.status === 0;
};

const collectFiles = (directory, predicate, found = []) => {
  if (!existsSync(directory)) {
    return found;
  }

  for (const entry of readdirSync(directory, { withFileTypes: true })) {
    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      collectFiles(fullPath, predicate, found);
      continue;
    }

    if (entry.isFile() && predicate(fullPath)) {
      found.push(fullPath);
    }
  }

  return found;
};

const runCommand = (command, args) => {
  statusInfo("executed", `Running ${command} ${args.join(" ")}`);
  const result = spawnSync(command, args, { stdio: "inherit" });
  if (typeof result.status === "number") {
    return result.status;
  }
  return 1;
};

const ensureTool = (command, label) => {
  if (!commandAvailable(command)) {
    const isCi = ["CI", "GITHUB_ACTIONS", "TF_BUILD", "BUILD_BUILDID"].some((name) => {
      const value = process.env[name];
      return Boolean(value && value !== "0" && value.toLowerCase() !== "false");
    });

    const installHint =
      command === "kubectl"
        ? "Install kubectl and ensure it is on PATH (Windows: winget install -e --id Kubernetes.kubectl)."
        : `Install ${label} and ensure it is on PATH.`;

    statusError(
      "blocked-missing-tooling",
      isCi
        ? `${label} tool is required on CI runner PATH. ${installHint}`
        : `${label} tool is required for local deploy validation. ${installHint}`,
    );
    process.exit(1);
  }
};

const ensureKubectl = () => {
  const versionJson = spawnSync("kubectl", ["version", "--client", "-o", "json"], {
    stdio: "pipe",
    encoding: "utf8",
  });
  const versionPlain = spawnSync("kubectl", ["version", "--client"], {
    stdio: "pipe",
    encoding: "utf8",
  });

  if (versionJson.status !== 0 && versionPlain.status !== 0) {
    const installHint =
      "Install kubectl and ensure it is on PATH (Windows: winget install -e --id Kubernetes.kubectl).";
    statusError(
      "blocked-missing-tooling",
      `kubectl tool is required for deploy validation. ${installHint}`,
    );
    process.exit(1);
  }

  const context = spawnSync("kubectl", ["config", "current-context"], {
    stdio: "pipe",
    encoding: "utf8",
  });
  const currentContext = context.stdout?.trim() ?? "";
  if (context.status !== 0 || currentContext.length === 0) {
    statusError(
      "blocked-missing-cluster-context",
      "kubectl is installed but no active cluster context is configured. Run `kubectl config get-contexts` and `kubectl config use-context <name>`.",
    );
    process.exit(1);
  }
};

const getDomainArtifacts = () => {
  const hasTerraform = existsSync(path.join(deployDir, "terraform"));
  const hasHelm = existsSync(path.join(deployDir, "helm"));
  const hasKubernetes = existsSync(path.join(deployDir, "kubernetes"));

  const terraformFiles = hasTerraform
    ? collectFiles(path.join(deployDir, "terraform"), (filePath) =>
        [".tf", ".tfvars"].some((extension) => filePath.endsWith(extension)),
      )
    : [];
  const helmChartFiles = hasHelm
    ? collectFiles(path.join(deployDir, "helm"), (filePath) => filePath.endsWith("Chart.yaml"))
    : [];
  const kubernetesManifestFiles = hasKubernetes
    ? collectFiles(path.join(deployDir, "kubernetes"), (filePath) =>
        [".yml", ".yaml"].some((extension) => filePath.endsWith(extension)),
      )
    : [];

  return {
    hasTerraform,
    hasHelm,
    hasKubernetes,
    terraformFiles,
    helmChartFiles,
    kubernetesManifestFiles,
  };
};

if (!task) {
  console.error("Usage: node scripts/orchestration/run-deploy-task.mjs <lint|validate>");
  process.exit(1);
}

if (!existsSync(deployDir)) {
  statusInfo("skipped-intentional", "No deploy directory found.");
  process.exit(0);
}

const artifacts = getDomainArtifacts();

if (!artifacts.hasTerraform && !artifacts.hasHelm && !artifacts.hasKubernetes) {
  statusInfo(
    "skipped-intentional",
    "Deploy domain has no deploy/{terraform|helm|kubernetes} subdomain yet.",
  );
  process.exit(0);
}

if (
  artifacts.terraformFiles.length === 0 &&
  artifacts.helmChartFiles.length === 0 &&
  artifacts.kubernetesManifestFiles.length === 0
) {
  statusInfo("skipped-intentional", "Deploy subdomains are scaffold-only (no artifacts yet).");
  process.exit(0);
}

if (task === "lint") {
  if (artifacts.hasTerraform) {
    if (artifacts.terraformFiles.length === 0) {
      statusInfo("skipped-intentional", "deploy/terraform has no .tf artifacts yet.");
    } else {
      ensureTool("terraform", "terraform");
      const terraformStatus = runCommand("terraform", [
        "fmt",
        "-check",
        "-recursive",
        "deploy/terraform",
      ]);
      if (terraformStatus !== 0) process.exit(terraformStatus);
    }
  }

  if (artifacts.hasHelm) {
    if (artifacts.helmChartFiles.length === 0) {
      statusInfo("skipped-intentional", "deploy/helm has no Chart.yaml artifacts yet.");
    } else {
      ensureTool("helm", "helm");
      for (const chartFile of artifacts.helmChartFiles) {
        const chartDir = path.dirname(chartFile);
        const helmStatus = runCommand("helm", ["lint", chartDir]);
        if (helmStatus !== 0) process.exit(helmStatus);
      }
    }
  }

  if (artifacts.hasKubernetes) {
    if (artifacts.kubernetesManifestFiles.length === 0) {
      statusInfo("skipped-intentional", "deploy/kubernetes has no YAML artifacts yet.");
    } else {
      ensureKubectl();
      const kubectlStatus = runCommand("kubectl", [
        "apply",
        "--dry-run=client",
        "-f",
        path.join("deploy", "kubernetes"),
      ]);
      if (kubectlStatus !== 0) process.exit(kubectlStatus);
    }
  }

  process.exit(0);
}

if (task === "validate") {
  if (artifacts.hasTerraform) {
    if (artifacts.terraformFiles.length === 0) {
      statusInfo("skipped-intentional", "deploy/terraform has no .tf artifacts yet.");
    } else {
      ensureTool("terraform", "terraform");
      const terraformStatus = runCommand("terraform", ["validate", "deploy/terraform"]);
      if (terraformStatus !== 0) process.exit(terraformStatus);
    }
  }

  if (artifacts.hasHelm) {
    if (artifacts.helmChartFiles.length === 0) {
      statusInfo("skipped-intentional", "deploy/helm has no Chart.yaml artifacts yet.");
    } else {
      ensureTool("helm", "helm");
      for (const chartFile of artifacts.helmChartFiles) {
        const chartDir = path.dirname(chartFile);
        const helmStatus = runCommand("helm", ["template", chartDir]);
        if (helmStatus !== 0) process.exit(helmStatus);
      }
    }
  }

  if (artifacts.hasKubernetes) {
    if (artifacts.kubernetesManifestFiles.length === 0) {
      statusInfo("skipped-intentional", "deploy/kubernetes has no YAML artifacts yet.");
    } else {
      ensureKubectl();
      const kubectlStatus = runCommand("kubectl", [
        "apply",
        "--dry-run=client",
        "-f",
        path.join("deploy", "kubernetes"),
      ]);
      if (kubectlStatus !== 0) process.exit(kubectlStatus);
    }
  }

  process.exit(0);
}

console.error(`Unsupported deploy task: ${task}`);
process.exit(1);
