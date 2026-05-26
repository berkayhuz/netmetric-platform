import en_US from "./messages/en.json";
import tr_TR from "./messages/tr.json";

export const availableMessageLocales = ["en-US", "tr-TR"] as const;

export const messageRegistry = {
  "en-US": en_US,
  "tr-TR": tr_TR,
} as const;
