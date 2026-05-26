export function createAriaId(prefix: string, value: string) {
  return `${prefix}-${value}`.toLowerCase().replaceAll(/[^a-z0-9-_]/g, "-");
}
