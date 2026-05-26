export const workspaceDomainsConfig = {
  root: {
    requiredFiles: ["pnpm-workspace.yaml", "pnpm-lock.yaml", "package.json", ".nvmrc"],
    packageManagerPrefix: "pnpm@",
  },
  domains: {
    frontend: {
      requiredDirectories: ["apps", "packages/frontend"],
      requiredWorkspacePatterns: ['- "apps/*"', '- "packages/frontend/*"'],
      requiredPackageManifests: [
        "package.json",
        "packages/frontend/config/package.json",
        "packages/frontend/eslint-config/package.json",
        "packages/frontend/tailwind-config/package.json",
        "packages/frontend/tsconfig/package.json",
        "packages/frontend/types/package.json",
        "packages/frontend/ui/package.json",
      ],
    },
  },
};
