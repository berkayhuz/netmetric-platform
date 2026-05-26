import { gzipSync } from "node:zlib";
import fs from "node:fs/promises";
import os from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { build } from "esbuild";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const packageRoot = path.resolve(__dirname, "..");

const scenarios = [
  {
    name: "core",
    source: `
      import { Button, Input } from "./src/index";
      export { Button, Input };
    `,
  },
  {
    name: "overlay",
    source: `
      import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "./src/client";
      export { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger };
    `,
  },
  {
    name: "data-heavy",
    source: `
      import { ChartContainer, ChartTooltip } from "./src/client";
      export { ChartContainer, ChartTooltip };
    `,
  },
  {
    name: "client-entry",
    source: `
      import { Checkbox, Dialog, Popover, Tooltip, Carousel, Calendar } from "./src/client";
      export { Checkbox, Dialog, Popover, Tooltip, Carousel, Calendar };
    `,
  },
  {
    name: "data-grid",
    source: `
      import type { ColumnDef } from "@tanstack/react-table";
      import { DataGrid } from "./src/client";

      type Row = { id: string; name: string };
      const columns: ColumnDef<Row, unknown>[] = [{ accessorKey: "name", header: "Name" }];
      const data: Row[] = [{ id: "1", name: "Acme" }];

      export { DataGrid, columns, data };
    `,
  },
];

function formatSize(bytes) {
  const kb = bytes / 1024;
  return `${kb.toFixed(2)} KiB`;
}

async function runScenario(tempDir, scenario) {
  const outFile = path.join(tempDir, `${scenario.name}.bundle.js`);

  await build({
    stdin: {
      contents: scenario.source,
      loader: "ts",
      resolveDir: packageRoot,
      sourcefile: `${scenario.name}.ts`,
    },
    outfile: outFile,
    bundle: true,
    format: "esm",
    platform: "browser",
    target: ["es2022"],
    minify: true,
    sourcemap: false,
    treeShaking: true,
    legalComments: "none",
    absWorkingDir: packageRoot,
    logLevel: "silent",
    external: ["react", "react-dom"],
  });

  const output = await fs.readFile(outFile);
  const gzip = gzipSync(output);
  return {
    scenario: scenario.name,
    rawBytes: output.byteLength,
    gzipBytes: gzip.byteLength,
  };
}

async function main() {
  const tempDir = await fs.mkdtemp(path.join(os.tmpdir(), "nm-ui-bundle-smoke-"));
  const results = [];

  try {
    for (const scenario of scenarios) {
      const result = await runScenario(tempDir, scenario);
      results.push(result);
    }
  } finally {
    await fs.rm(tempDir, { recursive: true, force: true });
  }

  console.log("UI bundle smoke report (informational, non-blocking):");
  for (const result of results) {
    console.log(
      `- ${result.scenario}: raw=${formatSize(result.rawBytes)} (${result.rawBytes} B), gzip=${formatSize(result.gzipBytes)} (${result.gzipBytes} B)`,
    );
  }

  console.log("Styles export presence (informational):");
  const styleExports = ["./styles/globals.css", "./styles/theme.css", "./styles/tokens.css"];
  for (const styleExport of styleExports) {
    const stylePath = path.join(packageRoot, "src", "styles", path.basename(styleExport));
    const exists = await fs
      .access(stylePath)
      .then(() => true)
      .catch(() => false);
    console.log(`- ${styleExport}: ${exists ? "present" : "missing"}`);
  }
}

await main();
