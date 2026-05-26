export function downloadConvertedImage(blob: Blob, fileName: string): void {
  const downloadUrl = URL.createObjectURL(blob);
  const link = document.createElement("a");

  link.href = downloadUrl;
  link.download = fileName;
  link.rel = "noopener";
  link.click();

  URL.revokeObjectURL(downloadUrl);
}
