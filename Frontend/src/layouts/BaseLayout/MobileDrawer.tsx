import type { ReactNode } from "react";
import { Drawer } from "antd";

type MobileDrawerProps = {
  open: boolean;
  onClose: () => void;
  brand: ReactNode;
  menu: ReactNode;
  userFooter?: ReactNode;
};

export function MobileDrawer({
  open,
  onClose,
  brand,
  menu,
  userFooter,
}: MobileDrawerProps) {
  return (
    <Drawer
      open={open}
      onClose={onClose}
      placement="left"
      size={280}
      title={brand}
      styles={{
        body: {
          padding: 0,
          display: "flex",
          flexDirection: "column",
        },
      }}
    >
      <div className="layout-drawer__inner">
        <div className="layout-sider__menu">{menu}</div>
        {userFooter}
      </div>
    </Drawer>
  );
}
