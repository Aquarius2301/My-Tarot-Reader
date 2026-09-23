/**
 * A single white coin batch usable by the user, mirroring the backend
 * `WhiteCoinBatchItem` record. Only non-expired batches with remaining coins
 * are returned, ordered by expiry (soonest first).
 */
export interface WhiteCoinBatchItem {
  id: string;
  amount: number;
  remainingAmount: number;
  expiredAt: string;
}

/** Wallet balance + active white coin batches, mirroring `GetWalletResult`. */
export interface GetWalletResult {
  whiteCoin: number;
  redCoin: number;
  whiteCoinBatches: WhiteCoinBatchItem[];
}

/** Convert request body, mirroring `ConvertRedToWhiteRequest`. 1 red = 2 white. */
export interface ConvertRedToWhiteRequest {
  redCoins: number;
}