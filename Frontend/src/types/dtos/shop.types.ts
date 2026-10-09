import type { PaymentOrderStatus } from "../enums.types";

/** A purchasable red-coin package exposed by the shop. */
export interface GetPackagesItem {
  code: string;
  redCoins: number;
  priceVnd: number;
}

/** Result of listing the purchasable packages. */
export interface GetPackagesResult {
  packages: GetPackagesItem[];
}

/** Request to create a payment order for a shop package. */
export interface CreatePaymentRequest {
  packageCode: string;
}

/** Result of creating a payment order. */
export interface CreatePaymentResult {
  orderId: string;
  orderCode: number;
  checkoutUrl: string;
  qrCode: string;
  amountVnd: number;
  redCoins: number;
}

/** Result of querying one of the user's payment orders. */
export interface GetOrderStatusResult {
  id: string;
  orderCode: number;
  status: PaymentOrderStatus;
  amountVnd: number;
  redCoins: number;
  paidAt: string | null;
}
