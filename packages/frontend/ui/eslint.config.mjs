import js from "@eslint/js";
import tseslint from "typescript-eslint";
import importPlugin from "eslint-plugin-import";
import jsxA11y from "eslint-plugin-jsx-a11y";
import reactHooks from "eslint-plugin-react-hooks";
import reactPlugin from "eslint-plugin-react";

const selfImportPatterns = ["@netmetric/ui", "@netmetric/ui/*", "@/*"];

const clientOnlyExportSources = [
  "./components/feedback/sonner",
  "./components/feedback/progress",
  "./components/data-display/avatar",
  "./components/data-display/calendar",
  "./components/data-display/chart",
  "./components/data-display/carousel",
  "./components/data-display/data-grid",
  "./components/data-display/data-grid/data-grid",
  "./components/data-display/data-table",
  "./components/data-display/data-table/data-table",
  "./components/data-display/data-table/data-table-column-header",
  "./components/data-display/data-table/data-table-empty-state",
  "./components/data-display/data-table/data-table-faceted-filter",
  "./components/data-display/data-table/data-table-pagination",
  "./components/data-display/data-table/data-table-skeleton",
  "./components/data-display/data-table/data-table-toolbar",
  "./components/data-display/data-table/data-table-view-options",
  "./components/overlay/alert-dialog",
  "./components/overlay/dialog",
  "./components/overlay/command",
  "./components/overlay/drawer",
  "./components/overlay/dropdown-menu",
  "./components/overlay/tooltip",
  "./components/overlay/sheet",
  "./components/overlay/popover",
  "./components/overlay/hover-card",
  "./components/forms/input-otp",
  "./components/navigation/menubar",
  "./components/navigation/navigation-menu",
  "./components/navigation/sidebar",
  "./components/navigation/tabs",
  "./components/layout/collapsible",
  "./components/layout/resizable",
  "./components/layout/scroll-area",
  "./components/primitives/checkbox",
  "./components/primitives/combobox",
  "./components/primitives/radio-group",
  "./components/primitives/direction-provider",
  "./components/primitives/switch",
  "./components/primitives/select",
  "./components/primitives/slider",
  "./components/primitives/toggle-group",
  "./components/primitives/toggle",
  "./components/theme/theme-provider",
  "./components/theme/theme-toggle",
  "./hooks/use-mounted",
  "./hooks/use-debounce",
  "./hooks/use-media-query",
  "./hooks/use-copy-to-clipboard",
  "./hooks/use-controllable-state",
];

export default [
  {
    ignores: ["node_modules/**", "dist/**", "build/**", "coverage/**"],
  },
  js.configs.recommended,
  ...tseslint.configs.recommended,
  {
    files: ["src/**/*.{ts,tsx}"],
    languageOptions: {
      parser: tseslint.parser,
      parserOptions: {
        project: "./tsconfig.json",
        tsconfigRootDir: import.meta.dirname,
        ecmaVersion: "latest",
        sourceType: "module",
      },
    },
    plugins: {
      "@typescript-eslint": tseslint.plugin,
      import: importPlugin,
      "jsx-a11y": jsxA11y,
      "react-hooks": reactHooks,
      react: reactPlugin,
    },
    settings: {
      react: {
        version: "detect",
      },
      "import/resolver": {
        typescript: {
          project: "./tsconfig.json",
        },
      },
    },
    rules: {
      "react-hooks/rules-of-hooks": "error",
      "react-hooks/exhaustive-deps": "error",

      "import/no-duplicates": "error",
      "import/newline-after-import": "error",
      "import/order": [
        "error",
        {
          groups: ["builtin", "external", "internal", "parent", "sibling", "index", "type"],
          "newlines-between": "always",
          alphabetize: { order: "asc", caseInsensitive: true },
        },
      ],

      "@typescript-eslint/no-unused-vars": [
        "error",
        {
          argsIgnorePattern: "^_",
          varsIgnorePattern: "^_",
          caughtErrorsIgnorePattern: "^_",
        },
      ],
      "@typescript-eslint/consistent-type-imports": ["error", { prefer: "type-imports" }],
      "@typescript-eslint/no-explicit-any": "error",
      "no-restricted-imports": [
        "error",
        {
          patterns: selfImportPatterns,
        },
      ],

      "jsx-a11y/aria-role": "error",
      "jsx-a11y/aria-props": "error",
      "jsx-a11y/aria-proptypes": "error",
      "jsx-a11y/role-has-required-aria-props": "error",

      "jsx-a11y/no-static-element-interactions": "warn",
      "jsx-a11y/click-events-have-key-events": "warn",
      "jsx-a11y/no-noninteractive-element-interactions": "warn",
      "jsx-a11y/anchor-is-valid": "warn",
    },
  },
  {
    files: ["src/index.ts"],
    rules: {
      "no-restricted-syntax": [
        "error",
        ...clientOnlyExportSources.map((source) => ({
          selector: `ExportNamedDeclaration[source.value='${source}']`,
          message:
            "Do not export client-only modules from src/index.ts. Export them from src/client.ts.",
        })),
        {
          selector: "ExportNamedDeclaration[source.value^='./hooks/']",
          message:
            "Hooks are client-facing by default and must be exported from src/client.ts, not src/index.ts.",
        },
      ],
    },
  },
];
