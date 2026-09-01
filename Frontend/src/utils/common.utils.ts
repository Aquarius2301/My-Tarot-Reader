/**
 * Converts a string to camel case by making the first character lowercase.
 * @param value  The string to be converted to camel case.
 * @returns  The camel case version of the input string.
 */
export function toCamelCase(value: string): string {
  return value.charAt(0).toLowerCase() + value.slice(1);
}
