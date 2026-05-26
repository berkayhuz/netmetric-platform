import type { ImageConverterMode } from "./image-validation";

export type ConvertedImage = {
  fileName: string;
  mimeType: string;
  blob: Blob;
};

const OUTPUT_MIME_BY_MODE: Record<ImageConverterMode, string> = {
  "png-to-jpg": "image/jpeg",
  "jpg-to-png": "image/png",
};

export async function convertImageWithCanvas(options: {
  imageBitmap: ImageBitmap;
  mode: ImageConverterMode;
  outputFileName: string;
  jpegQuality: number;
}): Promise<ConvertedImage> {
  const { imageBitmap, mode, outputFileName, jpegQuality } = options;

  const canvas = document.createElement("canvas");
  canvas.width = imageBitmap.width;
  canvas.height = imageBitmap.height;

  const context = canvas.getContext("2d");
  if (!context) {
    throw new Error("Canvas rendering is not available in this browser.");
  }

  if (mode === "png-to-jpg") {
    context.fillStyle = "#ffffff";
    context.fillRect(0, 0, canvas.width, canvas.height);
  }

  context.drawImage(imageBitmap, 0, 0);

  const outputMimeType = OUTPUT_MIME_BY_MODE[mode];

  const blob = await new Promise<Blob | null>((resolve) => {
    const quality = mode === "png-to-jpg" ? jpegQuality : undefined;
    canvas.toBlob(resolve, outputMimeType, quality);
  });

  if (!blob) {
    throw new Error("Could not create converted image output.");
  }

  return {
    blob,
    fileName: outputFileName,
    mimeType: outputMimeType,
  };
}
