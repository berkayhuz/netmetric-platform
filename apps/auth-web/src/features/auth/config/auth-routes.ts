export const authRoutes = {
  home: "/",
  login: "/login",
  register: "/register",
  forgotPassword: "/forgot-password",
  resetPassword: "/reset-password",
  confirmEmail: "/confirm-email",
  mfa: "/mfa",
  recoveryCode: "/recovery-code",
  workspaceSelect: "/workspace-select",
} as const;

export const externalRoutes = {
  publicSite: "https://netmetric.net",
  accountWeb: "https://account.netmetric.net",
  crmWeb: "https://crm.netmetric.net",
  toolsWeb: "https://tools.netmetric.net",
  support: "https://netmetric.net/contact",
  privacy: "https://netmetric.net/privacy",
  terms: "https://netmetric.net/terms",
} as const;

export type AuthRoute = (typeof authRoutes)[keyof typeof authRoutes];
