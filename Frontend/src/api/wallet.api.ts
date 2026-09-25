import type { ConvertRedToWhiteRequest, GetWalletResult } from "@/types";
import axiosClient from "./config.api";
import { API_URL } from "./url.api";

export const walletApi = {
  getWallet: (): Promise<GetWalletResult> =>
    axiosClient.get(API_URL.wallet.getWallet),

  convertRedToWhite: (request: ConvertRedToWhiteRequest): Promise<void> =>
    axiosClient.post(API_URL.wallet.convertRedToWhite, request),
};