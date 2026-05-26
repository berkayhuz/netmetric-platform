import { promises as fs } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const packageRoot = path.resolve(__dirname, "..");
const messagesDir = path.join(packageRoot, "src", "messages");
const outputPath = path.join(packageRoot, "src", "message-registry.generated.ts");
const supportedLanguagesPath = path.join(messagesDir, "supported-languages.json");

const files = await fs.readdir(messagesDir);
const messageFiles = new Set(
  files.filter((file) => file.endsWith(".json") && file !== "supported-languages.json"),
);

const supportedLanguages = JSON.parse(await fs.readFile(supportedLanguagesPath, "utf8"));
if (!Array.isArray(supportedLanguages) || supportedLanguages.length === 0) {
  throw new Error("messages/supported-languages.json must contain at least one language entry.");
}

const locales = supportedLanguages.map((entry) => {
  if (!entry || typeof entry !== "object") {
    throw new Error("messages/supported-languages.json contains an invalid entry.");
  }

  if (
    typeof entry.code !== "string" ||
    typeof entry.messageFile !== "string" ||
    typeof entry.nativeName !== "string" ||
    typeof entry.englishName !== "string"
  ) {
    throw new Error(
      "Each supported language entry must include string values for code, messageFile, nativeName, and englishName.",
    );
  }

  if (!messageFiles.has(entry.messageFile)) {
    throw new Error(
      `messages/supported-languages.json references missing message file '${entry.messageFile}'.`,
    );
  }

  return {
    code: entry.code,
    messageFile: entry.messageFile,
    importName: entry.code.replace(/[^A-Za-z0-9_]/g, "_"),
  };
});

if (!locales.some((entry) => entry.code.toLowerCase() === "en-us")) {
  throw new Error("messages/supported-languages.json must include the default locale 'en-US'.");
}

const importLines = locales.map(
  (locale) => `import ${locale.importName} from "./messages/${locale.messageFile}";`,
);
const localeList = locales.map((locale) => `"${locale.code}"`).join(", ");
const dictionaryLines = locales.map((locale) => `  "${locale.code}": ${locale.importName},`);

const content = `${importLines.join("\n")}

export const availableMessageLocales = [${localeList}] as const;

export const messageRegistry = {
${dictionaryLines.join("\n")}
} as const;
`;

await fs.writeFile(outputPath, content, "utf8");
