import type {
  CreateShopPackageRequest,
  GetAllShopPackagesResult,
  UpdateShopPackageRequest,
} from "@/types";
import axiosClient from "./config.api";
import { API_URL } from "./url.api";

export const shopAdminApi = {
  getPackages: (): Promise<GetAllShopPackagesResult> =>
    axiosClient.get(API_URL.admin.shopPackages),

  createPackage: (request: CreateShopPackageRequest): Promise<string> =>
    axiosClient.post(API_URL.admin.shopPackages, request),

  updatePackage: (
    id: string,
    request: UpdateShopPackageRequest,
  ): Promise<void> =>
    axiosClient.put(`${API_URL.admin.shopPackages}/${id}`, request),

  deletePackage: (id: string): Promise<void> =>
    axiosClient.delete(`${API_URL.admin.shopPackages}/${id}`),
};
