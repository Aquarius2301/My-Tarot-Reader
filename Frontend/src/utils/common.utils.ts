import FingerprintJS from "@fingerprintjs/fingerprintjs";

let cachedPromise: Promise<string> | null = null;
/**
 * Returns the cached FingerprintJS visitor id, loading it once on first call.
 * The visitor id is sent to the backend as the `X-Device-Id` header so refresh
 * tokens can be bound to the originating device (see backend TH2 handling).
 */
export const getVisitorId = async (): Promise<string> => {
  if (!cachedPromise) {
    cachedPromise = FingerprintJS.load()
      .then((fp) => fp.get())
      .then((r) => r.visitorId);
  }
  return cachedPromise;
};

/**
 * Converts a string to camel case by making the first character lowercase.
 * @param value  The string to be converted to camel case.
 * @returns  The camel case version of the input string.
 */
export function toCamelCase(value: string): string {
  return value.charAt(0).toLowerCase() + value.slice(1);
}
