import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const packageJsonPath = path.join(__dirname, "..", "package.json");
const packageJson = JSON.parse(fs.readFileSync(packageJsonPath, "utf-8"));
const packageRoot = path.join(__dirname, "..");

const allowedPublicExports = new Set([
  ".",
  "./client",
  "./app-background",
  "./styles/globals.css",
  "./styles/theme.css",
  "./styles/tokens.css",
]);

const requiredExports = [...allowedPublicExports];
const requiredClientSymbols = [
  "Avatar",
  "Calendar",
  "Carousel",
  "Checkbox",
  "DataGrid",
  "DirectionProvider",
  "Label",
];
const disallowedMainSymbols = new Set(requiredClientSymbols);
const allowedLibPublicExports = new Set([
  "./lib/utils",
  "./lib/accessibility",
  "./lib/format",
  "./lib/variants",
]);

function collectReExports(sourceText) {
  const reExports = [];
  let index = 0;

  while (index < sourceText.length) {
    const parsed = parseReExportAt(sourceText, index);
    if (!parsed) {
      break;
    }
    index = parsed.nextIndex;
    if (parsed.entry) {
      reExports.push(parsed.entry);
    }
  }

  return reExports;
}

function parseReExportAt(sourceText, index) {
  const exportIndex = sourceText.indexOf("export", index);
  if (exportIndex === -1) {
    return null;
  }

  const parsed = parseReExportFromIndex(sourceText, exportIndex);
  return {
    entry: parsed?.entry,
    nextIndex: parsed?.nextIndex ?? exportIndex + 1,
  };
}

function parseReExportFromIndex(sourceText, exportIndex) {
  let cursor = skipWhitespace(sourceText, exportIndex + "export".length);
  cursor = skipOptionalTypeKeyword(sourceText, cursor);

  if (sourceText[cursor] !== "{") {
    return null;
  }

  const namesStart = cursor + 1;
  const namesEnd = sourceText.indexOf("}", namesStart);
  if (namesEnd === -1) {
    return null;
  }

  cursor = skipWhitespace(sourceText, namesEnd + 1);
  if (!sourceText.startsWith("from", cursor) || !isBoundary(sourceText, cursor + "from".length)) {
    return { nextIndex: namesEnd + 1 };
  }

  cursor = skipWhitespace(sourceText, cursor + "from".length);
  const quote = sourceText[cursor];
  if (quote !== "'" && quote !== '"') {
    return { nextIndex: cursor + 1 };
  }

  const sourceStart = cursor + 1;
  const sourceEnd = sourceText.indexOf(quote, sourceStart);
  if (sourceEnd === -1) {
    return null;
  }

  return {
    nextIndex: sourceEnd + 1,
    entry: {
      source: sourceText.slice(sourceStart, sourceEnd),
      symbols: parseNamedSymbols(sourceText.slice(namesStart, namesEnd)),
    },
  };
}

function skipOptionalTypeKeyword(sourceText, index) {
  if (!sourceText.startsWith("type", index) || !isBoundary(sourceText, index + "type".length)) {
    return index;
  }

  return skipWhitespace(sourceText, index + "type".length);
}

function skipWhitespace(value, index) {
  let cursor = index;
  while (cursor < value.length && isWhitespace(value[cursor])) {
    cursor += 1;
  }
  return cursor;
}

function isWhitespace(char) {
  return char === " " || char === "\n" || char === "\r" || char === "\t";
}

function isBoundary(value, index) {
  const char = value[index];
  return !char || !isIdentifierChar(char);
}

function isIdentifierChar(char) {
  const code = char.codePointAt(0) ?? -1;
  return (
    (code >= 65 && code <= 90) ||
    (code >= 97 && code <= 122) ||
    (code >= 48 && code <= 57) ||
    char === "_" ||
    char === "$"
  );
}

function parseNamedSymbols(value) {
  return value
    .split(",")
    .map((item) => item.trim())
    .filter(Boolean)
    .map((item) => {
      const typeTrimmed = item.startsWith("type ") ? item.slice("type ".length).trim() : item;
      const aliasParts = typeTrimmed.split(" as ");
      return aliasParts[aliasParts.length - 1].trim();
    });
}

function resolveSourceFile(fromFile, sourceSpecifier) {
  const base = path.resolve(path.dirname(fromFile), sourceSpecifier);
  const candidates = [
    `${base}.ts`,
    `${base}.tsx`,
    path.join(base, "index.ts"),
    path.join(base, "index.tsx"),
    base,
  ];

  for (const candidate of candidates) {
    if (fs.existsSync(candidate) && fs.statSync(candidate).isFile()) {
      return candidate;
    }
  }

  throw new Error(`Unable to resolve export target: ${sourceSpecifier} from ${fromFile}`);
}

function hasUseClientDirective(filePath) {
  const source = fs.readFileSync(filePath, "utf-8");
  for (const line of source.split("\n")) {
    const trimmed = line.trim();
    if (trimmed.length === 0 || trimmed.startsWith("//")) {
      continue;
    }

    return (
      trimmed === '"use client"' ||
      trimmed === "'use client'" ||
      trimmed === '"use client";' ||
      trimmed === "'use client';"
    );
  }

  return false;
}

function walkFiles(dirPath, output = []) {
  for (const entry of fs.readdirSync(dirPath, { withFileTypes: true })) {
    const fullPath = path.join(dirPath, entry.name);
    if (entry.isDirectory()) {
      walkFiles(fullPath, output);
      continue;
    }

    if (entry.isFile() && (fullPath.endsWith(".ts") || fullPath.endsWith(".tsx"))) {
      output.push(fullPath);
    }
  }
  return output;
}

for (const exportKey of requiredExports) {
  const exportPath = packageJson.exports?.[exportKey];
  if (typeof exportPath !== "string") {
    throw new TypeError(`Missing export map entry: ${exportKey}`);
  }

  const resolvedPath = path.join(__dirname, "..", exportPath);
  if (!fs.existsSync(resolvedPath)) {
    throw new Error(`Export path does not exist for ${exportKey}: ${exportPath}`);
  }
}

for (const exportKey of Object.keys(packageJson.exports ?? {})) {
  if (!allowedPublicExports.has(exportKey)) {
    throw new Error(`Unexpected public export entry found in package.json: ${exportKey}`);
  }
}

const publicSpecifiers = [
  "@netmetric/ui",
  "@netmetric/ui/client",
  "@netmetric/ui/app-background",
  "@netmetric/ui/styles/globals.css",
  "@netmetric/ui/styles/theme.css",
  "@netmetric/ui/styles/tokens.css",
];

for (const specifier of publicSpecifiers) {
  let resolved;
  try {
    resolved = import.meta.resolve(specifier);
  } catch (error) {
    throw new Error(`Failed to resolve public specifier: ${specifier}\n${String(error)}`);
  }

  if (!resolved?.startsWith("file://")) {
    throw new Error(`Unexpected resolution for ${specifier}: ${resolved}`);
  }
}

const mainEntryPath = path.join(packageRoot, packageJson.exports["."]);
const mainEntrySource = fs.readFileSync(mainEntryPath, "utf-8");
const mainReExports = collectReExports(mainEntrySource);

for (const reExport of mainReExports) {
  if (reExport.source.startsWith("./hooks/")) {
    throw new Error(`Main entry exports a hook source, must be client-only: ${reExport.source}`);
  }

  if (reExport.source.startsWith("./lib/") && !allowedLibPublicExports.has(reExport.source)) {
    throw new Error(`Main entry exports internal lib helper: ${reExport.source}`);
  }

  const targetFile = resolveSourceFile(mainEntryPath, reExport.source);
  if (hasUseClientDirective(targetFile)) {
    throw new Error(`Main entry re-exports a client-only module: ${reExport.source}`);
  }

  for (const symbol of reExport.symbols) {
    if (disallowedMainSymbols.has(symbol)) {
      throw new Error(`Main entry exports client-only symbol: ${symbol}`);
    }
  }
}

const clientEntryPath = path.join(packageRoot, packageJson.exports["./client"]);
const clientEntrySource = fs.readFileSync(clientEntryPath, "utf-8");
const clientReExports = collectReExports(clientEntrySource);

for (const reExport of clientReExports) {
  if (reExport.source.startsWith("./lib/") && !allowedLibPublicExports.has(reExport.source)) {
    throw new Error(`Client entry exports internal lib helper: ${reExport.source}`);
  }
}

for (const symbol of requiredClientSymbols) {
  const exported = new RegExp(String.raw`\b${symbol}\b`, "m").test(clientEntrySource);
  if (!exported) {
    throw new Error(`Missing client symbol in client entry source: ${symbol}`);
  }
}

const srcFiles = walkFiles(path.join(packageRoot, "src"));
for (const filePath of srcFiles) {
  const source = fs.readFileSync(filePath, "utf-8");
  if (/from\s+["']@netmetric\/ui(?:\/[^"']*)?["']/.test(source)) {
    throw new Error(`Package self-reference import found: ${path.relative(packageRoot, filePath)}`);
  }
}

console.log("exports smoke check passed");
