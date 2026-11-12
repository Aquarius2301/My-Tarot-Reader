import {
  BarChartOutlined,
  DashboardOutlined,
  SettingOutlined,
  TeamOutlined,
} from "@ant-design/icons";

import type { LayoutMenuItem } from "@/constants/theme.constants";
import { BaseLayout } from "@/layouts/BaseLayout";
import { useTranslation } from "react-i18next";

const menuItems: LayoutMenuItem[] = [
  { key: "/admin", label: "Dashboard", icon: <DashboardOutlined /> },
  { key: "/admin/users", label: "Người dùng", icon: <TeamOutlined /> },
  { key: "/admin/reports", label: "Báo cáo", icon: <BarChartOutlined /> },
  { key: "/admin/settings", label: "Cài đặt", icon: <SettingOutlined /> },
];

export function AdminLayout({ title }: { title?: string }) {
  const { t } = useTranslation();

  return (
    <BaseLayout
      variant="admin"
      title={title ? t(title) : "Admin"}
      items={menuItems}
    />
  );
}
