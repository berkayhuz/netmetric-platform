const IMAGE_CATEGORY = "image";

export function getToolRoutePath(slug: string): string {
  if (slug === "png-to-jpg") {
    return "/image/png-to-jpg";
  }

  if (slug === "jpg-to-png") {
    return "/image/jpg-to-png";
  }

  if (slug === "qr-generator") {
    return "/qr-generator";
  }

  if (slug.endsWith("-to-jpg") || slug.endsWith("-to-png") || slug.endsWith("-resize")) {
    return `/${IMAGE_CATEGORY}/${slug}`;
  }

  return `/${slug}`;
}
