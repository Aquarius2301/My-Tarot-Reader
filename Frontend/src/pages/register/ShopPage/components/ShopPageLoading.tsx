import { Col, Row } from "antd";
import { PackageCardLoading } from "./PackageCardLoading";

/** Skeleton grid shown while the shop packages are loading. */
export function ShopPageLoading() {
  return (
    <Row
      gutter={[16, 16]}
      role="status"
      aria-busy
      aria-label="Loading shop packages"
    >
      {Array.from({ length: 6 }).map((_, index) => (
        <Col xs={24} sm={12} lg={8} key={index}>
          <PackageCardLoading />
        </Col>
      ))}
    </Row>
  );
}
