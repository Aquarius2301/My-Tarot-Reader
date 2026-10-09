import type {
  CreatePaymentRequest,
  CreatePaymentResult,
  GetOrderStatusResult,
  GetPackagesResult,
} from "@/types";
import axiosClient from "./config.api";
import { API_URL } from "./url.api";

export const shopApi = {
  getPackages: (): Promise<GetPackagesResult> =>
    axiosClient.get(API_URL.shop.packages),

  // Returns the created resource (checkout url + QR) so the client can open
  // PayOS and then poll getOrderStatus until the order settles.
  createOrder: (request: CreatePaymentRequest): Promise<CreatePaymentResult> =>
    axiosClient.post(API_URL.shop.createOrder, request),

  getOrderStatus: (orderId: string): Promise<GetOrderStatusResult> =>
    axiosClient.get(`${API_URL.shop.getOrder}/${orderId}`),
};
