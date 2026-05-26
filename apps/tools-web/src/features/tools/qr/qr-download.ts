export const DEFAULT_QR_FILENAME = "netmetric-qr-code.png";

export function downloadQrPng(dataUrl: string, fileName: string = DEFAULT_QR_FILENAME): void {
  const link = document.createElement("a");
  link.href = dataUrl;
  link.download = fileName;
  link.rel = "noopener";
  link.click();
}
