import type { GetPackagesItem } from "@/types";
import { Col, Row } from "antd";
import { PackageCard, PackageCardLoading } from "./PackageCard";

export interface ShopPackageListProps {
  packages: GetPackagesItem[];
  isLoading?: boolean;
  isPending?: boolean;
  pendingPackageCode?: string;
  onBuy: (pkg: GetPackagesItem) => void;
}

/** Renders the purchasable packages as a responsive card grid. */
export function ShopPackageList({
  packages,
  isLoading = false,
  isPending = false,
  pendingPackageCode,
  onBuy,
}: ShopPackageListProps) {
  if (isLoading) {
    return (
      <Row
        gutter={[16, 16]}
        role="status"
        aria-busy
        // aria-label={t("page.shop.loadingPackages")}
      >
        {Array.from({ length: 6 }).map((_, index) => (
          <Col xs={24} sm={12} lg={8} key={index}>
            <PackageCardLoading />
          </Col>
        ))}
      </Row>
    );
  }

  return (
    <Row gutter={[16, 16]}>
      {packages.map((pkg) => (
        <Col xs={24} sm={12} lg={8} key={pkg.code}>
          <PackageCard
            pkg={pkg}
            loading={isPending && pendingPackageCode === pkg.code}
            disabled={isPending}
            onBuy={() => onBuy(pkg)}
          />
        </Col>
      ))}
    </Row>
  );
}
