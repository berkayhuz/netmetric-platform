import * as React from "react";

const THEME_STORAGE_KEY = "netmetric-theme";
const THEME_COOKIE_KEY = "netmetric-theme";

export function getThemeInitScript(serverTheme?: "light" | "dark" | "system"): string {
  const safeServerTheme =
    serverTheme === "light" || serverTheme === "dark" || serverTheme === "system"
      ? serverTheme
      : "system";

  return `(function(){try{var storageKey='${THEME_STORAGE_KEY}';var cookieKey='${THEME_COOKIE_KEY}';var serverTheme='${safeServerTheme}';var normalize=function(value){if(!value)return null;value=String(value).trim().toLowerCase();if(value==='light'||value==='dark'||value==='system')return value;if(value==='default')return 'system';return null;};var cookieMatch=document.cookie.match(new RegExp('(?:^|; )'+cookieKey+'=([^;]*)'));var cookieTheme=normalize(cookieMatch?decodeURIComponent(cookieMatch[1]):null);var storedRaw=localStorage.getItem(storageKey);var stored=normalize(storedRaw);var theme=cookieTheme||(serverTheme!=='system'?serverTheme:stored)||serverTheme;if(storedRaw!==theme)localStorage.setItem(storageKey,theme);var dark=theme==='dark'||(theme==='system'&&window.matchMedia('(prefers-color-scheme: dark)').matches);var root=document.documentElement;root.classList.toggle('dark',dark);root.style.colorScheme=dark?'dark':'light';}catch(e){}})();`;
}

export function ThemeInitScript(): React.JSX.Element {
  return <script dangerouslySetInnerHTML={{ __html: getThemeInitScript() }} />;
}
