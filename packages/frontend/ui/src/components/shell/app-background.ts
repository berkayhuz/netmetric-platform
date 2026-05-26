import fs from "node:fs";
import path from "node:path";

import type * as React from "react";

const allowedImageDataUrlPattern =
  /^data:image\/(?:png|jpe?g|webp|gif|avif);base64,[A-Za-z0-9+/=\r\n]+$/;

type AppBackgroundStyle = React.CSSProperties & {
  "--netmetric-app-background-image-light"?: string;
  "--netmetric-app-background-image-dark"?: string;
};

export type AppBackgroundDataUrls = {
  light: string;
  dark: string;
};

function normalizeDataUrl(value: string): string {
  return value.trim().replace(/\s+/g, "");
}

export function sanitizeAppBackgroundDataUrl(value: string, label: string): string {
  const normalized = normalizeDataUrl(value);
  if (!allowedImageDataUrlPattern.test(normalized)) {
    throw new Error(`Invalid ${label} app background image data URL.`);
  }

  return normalized;
}

export function createAppBackgroundStyle(backgrounds: AppBackgroundDataUrls): AppBackgroundStyle {
  const light = sanitizeAppBackgroundDataUrl(backgrounds.light, "light");
  const dark = sanitizeAppBackgroundDataUrl(backgrounds.dark, "dark");

  return {
    "--netmetric-app-background-image-light": `url(${JSON.stringify(light)})`,
    "--netmetric-app-background-image-dark": `url(${JSON.stringify(dark)})`,
  };
}

const appBackgroundAssetsPath = path.join("packages", "frontend", "ui", "assets");

function findWorkspaceAsset(fileName: string, startDirectory = process.cwd()): string {
  let current = path.resolve(startDirectory);

  for (let depth = 0; depth < 8; depth += 1) {
    const candidates = [
      path.join(current, appBackgroundAssetsPath, fileName),
      path.join(current, "NetMetric", appBackgroundAssetsPath, fileName),
    ];

    for (const candidate of candidates) {
      if (fs.existsSync(candidate)) {
        return candidate;
      }
    }

    const parent = path.dirname(current);
    if (parent === current) {
      break;
    }

    current = parent;
  }

  throw new Error(
    `Missing shared app background asset: ${path.join(appBackgroundAssetsPath, fileName)}`,
  );
}

export function loadAppBackgroundDataUrls(startDirectory = process.cwd()): AppBackgroundDataUrls {
  return {
    light: fs.readFileSync(findWorkspaceAsset("data_image_white.txt", startDirectory), "utf8"),
    dark: fs.readFileSync(findWorkspaceAsset("data_image_black.txt", startDirectory), "utf8"),
  };
}

export function loadAppBackgroundStyle(startDirectory = process.cwd()): AppBackgroundStyle {
  return createAppBackgroundStyle(loadAppBackgroundDataUrls(startDirectory));
}
