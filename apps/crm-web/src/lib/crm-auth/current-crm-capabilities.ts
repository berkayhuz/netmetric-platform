import "server-only";

import { Buffer } from "node:buffer";

import {
  createCrmCapabilities,
  emptyCrmCapabilities,
  type CrmCapabilities,
} from "./crm-capabilities";
import { getCrmAccessToken } from "./crm-auth-headers";

type JwtPayload = {
  permission?: string | string[];
  permissions?: string | string[];
};

function toPermissionArray(value: string | string[] | undefined): string[] {
  if (Array.isArray(value)) {
    return value;
  }

  if (typeof value === "string") {
    return value.split(",");
  }

  return [];
}

function decodePermissionsFromAccessToken(token: string | undefined): string[] {
  const payload = token?.split(".")[1];
  if (!payload) {
    return [];
  }

  try {
    const json = Buffer.from(payload.replace(/-/g, "+").replace(/_/g, "/"), "base64").toString(
      "utf8",
    );
    const parsed = JSON.parse(json) as JwtPayload;
    return [...toPermissionArray(parsed.permission), ...toPermissionArray(parsed.permissions)];
  } catch {
    return [];
  }
}

export async function getCurrentCrmCapabilities(): Promise<CrmCapabilities> {
  const token = await getCrmAccessToken();
  if (!token) {
    return emptyCrmCapabilities;
  }

  return createCrmCapabilities(decodePermissionsFromAccessToken(token));
}
