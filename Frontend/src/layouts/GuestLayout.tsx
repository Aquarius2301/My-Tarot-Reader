import { GoogleOutlined, HomeOutlined, StarOutlined } from "@ant-design/icons";
import type { LayoutMenuItem } from "@/constants/theme.constants";
import { BaseLayout } from "@/layouts/BaseLayout";
import { useTranslation } from "react-i18next";
import { WEB_URL } from "@/routes";

type GuestLayoutProps = {
  title?: string;
};

export function GuestLayout({ title }: GuestLayoutProps) {
  const { t } = useTranslation();

  const menuItems: LayoutMenuItem[] = [
    {
      key: WEB_URL.guestHome,
      label: t("page.home.title"),
      icon: <HomeOutlined />,
    },
    {
      key: WEB_URL.guestTarot,
      label: t("page.tarot.title"),
      icon: <StarOutlined />,
    },
    {
      key: WEB_URL.login,
      label: t("page.login.title"),
      icon: <GoogleOutlined />,
    },
  ];

  return (
    <BaseLayout
      variant="guest"
      title={title ? t(title) : t("page.home.title")}
      items={menuItems}
    />
  );
}
