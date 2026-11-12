import { Button, Grid, Result } from "antd";
import { ArrowLeftOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { WEB_URL } from "@/routes";
import type { PaymentOrderStatus } from "@/types";

const { useBreakpoint } = Grid;

const FAILED_STATUS_KEY: Record<
  Exclude<PaymentOrderStatus, "pending" | "paid">,
  string
> = {
  cancelled: "page.shop.status.cancelled",
  expired: "page.shop.status.expired",
  failed: "page.shop.status.failed",
};

export interface CheckoutFailedResultProps {
  status: Exclude<PaymentOrderStatus, "pending" | "paid">;
}

/** Terminal failure state: the order was cancelled, expired, or failed. */
export function CheckoutFailedResult({ status }: CheckoutFailedResultProps) {
  const { t } = useTranslation();
  const screens = useBreakpoint();
  const isMobile = !screens.md;
  const navigate = useNavigate();

  return (
    <Result
      status="error"
      title={t("page.shop.failedTitle")}
      subTitle={t(FAILED_STATUS_KEY[status])}
      extra={
        <Button
          type="primary"
          size={isMobile ? "large" : "middle"}
          block={isMobile}
          icon={<ArrowLeftOutlined />}
          onClick={() => navigate(WEB_URL.shop)}
        >
          {t("page.shop.backToShop")}
        </Button>
      }
    />
  );
}
