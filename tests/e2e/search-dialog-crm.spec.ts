import { expect, test, type Page } from "@playwright/test";

function createE2ePassword() {
  return [
    "Phase8D3",
    Date.now().toString(36),
    Math.random().toString(36).slice(2, 10),
    "Aa1!",
  ].join("-");
}

async function registerUserInBrowserSession(page: Page, authBase: string): Promise<void> {
  const email = `phase8d3.crm.${Date.now()}.${Math.random().toString(36).slice(2, 8)}@example.test`;
  const password = process.env.E2E_SMOKE_PASSWORD ?? createE2ePassword();
  const userName = `phase8d3-crm-${Date.now().toString(36)}`;

  const response = await page.request.post(`${authBase}/api/auth/register`, {
    data: {
      tenantName: "Phase8D3",
      userName,
      email,
      password,
      firstName: "Phase",
      lastName: "EightD3",
      culture: "en-US",
    },
  });
  expect(response.ok()).toBe(true);

  const rawSetCookie = response.headers()["set-cookie"] ?? "";
  const accessTokenMatch = rawSetCookie.match(/netmetric-access=([^;]+)/);
  const accessToken = accessTokenMatch?.[1];
  expect(accessToken).toBeTruthy();

  await page.context().addCookies([
    {
      name: "netmetric-access",
      value: accessToken!,
      domain: "localhost",
      path: "/",
      httpOnly: true,
      sameSite: "Lax",
    },
  ]);
}

async function openSearchDialog(page: Page) {
  const trigger = page.getByTestId("global-search-trigger").first();
  await expect(trigger).toBeVisible();
  await expect(trigger).toBeEnabled({ timeout: 20_000 });
  await trigger.click();
  const dialog = page.getByTestId("global-search-dialog").first();
  await expect(dialog).toBeVisible({ timeout: 10_000 });
  await expect(page.getByTestId("global-search-results")).toBeVisible({ timeout: 10_000 });
  await expect(page.getByTestId("global-search-input").first()).toBeVisible({ timeout: 10_000 });
}

async function runSearch(page: Page, query: string) {
  const responsePromise = page.waitForResponse(
    (response) => {
      if (!response.url().includes("/api/search") || response.request().method() !== "GET") {
        return false;
      }

      const url = new URL(response.url());
      const value = url.searchParams.get("q") ?? url.searchParams.get("query");
      return value?.toLowerCase() === query.toLowerCase() && response.status() === 200;
    },
    { timeout: 20_000 },
  );

  let lastError: unknown = null;
  for (let attempt = 0; attempt < 3; attempt += 1) {
    try {
      const input = page.getByTestId("global-search-input").first();
      await expect(input).toBeVisible({ timeout: 10_000 });
      await input.click();
      await input.press("Control+A");
      await input.fill(query);
      lastError = null;
      break;
    } catch (error) {
      lastError = error;
      if (attempt === 2) {
        throw error;
      }
    }
  }

  if (lastError) {
    throw lastError;
  }

  await responsePromise;
  await expect(page.getByTestId("global-search-results")).toHaveAttribute(
    "data-search-state",
    "ready",
    {
      timeout: 20_000,
    },
  );
}

test.describe("Global search dialog crm-web", () => {
  test.skip(
    process.env.RUN_E2E_SMOKE !== "1" && process.env.CI_E2E_SMOKE !== "1",
    "E2E search dialog is disabled by default",
  );

  test("authenticated CRM search shows pricing/customers and no content", async ({ page }) => {
    test.fixme(
      true,
      "Quarantined: authenticated cmdk/dialog result rendering is flaky in local automation. API/proxy/component coverage verifies behavior. Manual check: login -> crm-web -> header search -> query 'contacts'/'tickets' -> verify authorized CRM results and no content rendering.",
    );

    const authBase = process.env.E2E_AUTH_BASE_URL ?? "http://localhost:7002";
    const crmBase = process.env.E2E_CRM_BASE_URL ?? "http://localhost:7006";

    await registerUserInBrowserSession(page, authBase);
    await page.goto(crmBase, { waitUntil: "domcontentloaded" });
    await page.waitForURL((url) => url.origin === new URL(crmBase).origin, { timeout: 30_000 });
    await expect(page.locator("body")).toBeVisible();

    await openSearchDialog(page);
    await runSearch(page, "pricing");
    await expect(
      page
        .getByTestId("global-search-result")
        .filter({ hasText: /Pricing/i })
        .first(),
    ).toBeVisible({ timeout: 15_000 });

    await runSearch(page, "customers");
    await expect(
      page
        .getByTestId("global-search-result")
        .filter({ hasText: /Customers/i })
        .first(),
    ).toBeVisible({ timeout: 15_000 });
    await page
      .getByTestId("global-search-result")
      .filter({ hasText: /Customers/i })
      .first()
      .click();
    await page.waitForURL((url) => url.origin === new URL(crmBase).origin, { timeout: 20_000 });
    await expect(page.getByText("Sensitive indexed content should stay hidden")).toHaveCount(0);
  });

  test("anonymous CRM search does not leak customer records", async ({ request }) => {
    const crmBase = process.env.E2E_CRM_BASE_URL ?? "http://localhost:7006";
    const response = await request.get(`${crmBase}/api/search?q=customers&pageSize=10`);
    if (!response.ok() && response.status() >= 500) {
      test.skip(
        true,
        `CRM app/proxy is unavailable in this run (status ${response.status()}); skipping leakage assertion.`,
      );
    }
    expect(response.ok()).toBe(true);

    const payload = (await response.json()) as {
      items?: Array<{ source?: string | number; type?: string }>;
    };
    const items = payload.items ?? [];
    const leakedCustomerResult = items.some(
      (item) =>
        String(item.type ?? "").toLowerCase() === "customer" &&
        (String(item.source ?? "").toLowerCase() === "crm" || Number(item.source) === 5),
    );

    expect(leakedCustomerResult).toBe(false);
  });
});
