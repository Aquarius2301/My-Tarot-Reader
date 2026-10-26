import { shopAdminApi } from "@/api";
import type {
  CreateShopPackageRequest,
  UpdateShopPackageRequest,
} from "@/types";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  GET_ADMIN_SHOP_PACKAGES_QUERY_KEY,
  GET_PACKAGES_QUERY_KEY,
  SHOP_ADMIN_QUERY_KEY,
} from "./queryKey";

export const useGetAdminShopPackages = () =>
  useQuery({
    queryKey: GET_ADMIN_SHOP_PACKAGES_QUERY_KEY,
    queryFn: shopAdminApi.getPackages,
  });

const useInvalidateShopPackages = () => {
  const queryClient = useQueryClient();

  return () => {
    queryClient.invalidateQueries({ queryKey: SHOP_ADMIN_QUERY_KEY });
    queryClient.invalidateQueries({ queryKey: GET_PACKAGES_QUERY_KEY });
  };
};

export const useCreateShopPackage = () => {
  const invalidate = useInvalidateShopPackages();

  return useMutation({
    mutationFn: (request: CreateShopPackageRequest): Promise<string> =>
      shopAdminApi.createPackage(request),
    onSuccess: invalidate,
  });
};

export const useUpdateShopPackage = () => {
  const invalidate = useInvalidateShopPackages();

  return useMutation({
    mutationFn: ({
      id,
      request,
    }: {
      id: string;
      request: UpdateShopPackageRequest;
    }): Promise<void> => shopAdminApi.updatePackage(id, request),
    onSuccess: invalidate,
  });
};

export const useDeleteShopPackage = () => {
  const invalidate = useInvalidateShopPackages();

  return useMutation({
    mutationFn: (id: string): Promise<void> => shopAdminApi.deletePackage(id),
    onSuccess: invalidate,
  });
};
