import { test, expect } from "@playwright/test";

function createSmokePassword() {
  return ["Smoke", Date.now().toString(36), Math.random().toString(36).slice(2, 10), "Aa1!"].join(
    "-",
  );
}

test("register-confirm-login-preferences-crm-logout smoke", async ({ page, request }) => {
  test.skip(
    process.env.RUN_E2E_SMOKE !== "1" && process.env.CI_E2E_SMOKE !== "1",
    "Smoke disabled by default",
  );

  const authBase = process.env.E2E_AUTH_BASE_URL ?? "http://localhost:7002";
  const accountBase = process.env.E2E_ACCOUNT_BASE_URL ?? "http://localhost:7004";
  const crmBase = process.env.E2E_CRM_BASE_URL ?? "http://localhost:7006";

  const email = `smoke.${Date.now()}@example.test`;
  const password = process.env.E2E_SMOKE_PASSWORD ?? createSmokePassword();

  await request.post(`${authBase}/api/auth/register`, {
    data: {
      tenantName: "Smoke",
      userName: "smoke-user",
      email,
      password,
      firstName: "Smoke",
      lastName: "User",
      culture: "en-US",
    },
  });

  await request.post(`${authBase}/api/auth/confirm-email`, {
    data: {
      tenantId: process.env.E2E_TENANT_ID ?? "00000000-0000-0000-0000-000000000000",
      userId: process.env.E2E_USER_ID ?? "00000000-0000-0000-0000-000000000000",
      token: process.env.E2E_CONFIRM_TOKEN ?? "test-token",
    },
  });

  await request.post(`${authBase}/api/auth/login`, {
    data: {
      tenantId: process.env.E2E_TENANT_ID ?? "00000000-0000-0000-0000-000000000000",
      emailOrUserName: email,
      password,
    },
  });

  await page.goto(accountBase);
  await expect(page).toHaveTitle(/NetMetric/i);

  await page.goto(crmBase);
  await expect(page.locator("body")).toBeVisible();

  await request.post(`${authBase}/api/auth/logout`, { data: {} });
});
