import { Menu } from "antd";

import type { LayoutMenuItem } from "@/constants";
import { findActiveMenu } from "./utils";

type LayoutMenuProps = {
  items: LayoutMenuItem[];
  pathname: string;
  isDark: boolean;
  onNavigate: (key: string) => void;
};

export function LayoutMenu({
  items,
  pathname,
  isDark,
  onNavigate,
}: LayoutMenuProps) {
  const activeMenu = findActiveMenu(items, pathname);
  const selectedKeys = activeMenu ? [activeMenu.key] : [];

  return (
    <Menu
      key={pathname}
      mode="inline"
      theme={isDark ? "dark" : "light"}
      items={items}
      selectedKeys={selectedKeys}
      defaultOpenKeys={activeMenu?.openKeys}
      onClick={({ key }) => onNavigate(key)}
      style={{ background: "transparent", borderInlineEnd: 0 }}
    />
  );
}
