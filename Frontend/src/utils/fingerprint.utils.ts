import FingerprintJS from "@fingerprintjs/fingerprintjs";

let cached: string | null = null;

/**
 * Returns the cached FingerprintJS visitor id, loading it once on first call.
 * The visitor id is sent to the backend as the `X-Device-Id` header so refresh
 * tokens can be bound to the originating device (see backend TH2 handling).
 */
export const getVisitorId = async (): Promise<string> => {
  if (cached) return cached;

  const fp = await FingerprintJS.load();
  cached = (await fp.get()).visitorId;

  return cached;
};
