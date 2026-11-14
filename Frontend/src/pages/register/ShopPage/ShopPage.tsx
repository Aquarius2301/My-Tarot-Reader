import { App } from "antd";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { AppTitle, EmptyView, ErrorView } from "@/components";
import { useCreateOrder, useGetPackages } from "@/hooks/api";
import { WEB_URL } from "@/routes";
import type { GetPackagesItem } from "@/types";
import { getErrorMessage, saveCheckoutPayload } from "@/utils";
import { ShopPackageList } from "./ShopPackageList";

/**
 * Shop page: lists the purchasable red-coin packages as cards. Buying one
 * creates a PayOS order and hands the checkout payload over to the checkout
 * page, which polls the order until the payment settles.
 */
export default function ShopPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { message } = App.useApp();
  const { data, isLoading, isError, refetch } = useGetPackages();
  const { mutate, isPending, variables } = useCreateOrder();

  const handleBuy = (pkg: GetPackagesItem) => {
    mutate(
      { packageCode: pkg.code },
      {
        onSuccess: (order) => {
          saveCheckoutPayload(order.orderId, {
            checkoutUrl: order.checkoutUrl,
            qrCode: order.qrCode,
          });
          navigate(`${WEB_URL.shopCheckout}/${order.orderId}`, {
            state: order,
          });
        },
        onError: (error) => message.error(getErrorMessage(error)),
      },
    );
  };

  if (isError) {
    return <ErrorView type="server" onRetry={refetch} />;
  }

  if (data?.packages.length === 0) {
    return <EmptyView description={t("page.shop.empty")} />;
  }

  return (
    <div>
      <AppTitle
        title={t("page.shop.title")}
        subtitle={t("page.shop.subtitle")}
        isLoading={isLoading}
      />

      <ShopPackageList
        packages={data?.packages}
        isLoading={isLoading}
        isPending={isPending}
        pendingPackageCode={variables?.packageCode}
        onBuy={handleBuy}
      />
    </div>
  );
}
