import type { LayoutMenuItem } from "@/constants/theme.constants";
import { BaseLayout } from "@/layouts/BaseLayout";
import { WEB_URL } from "@/routes";
import { useTranslation } from "react-i18next";
import {
  HomeOutlined,
  BookOutlined,
  StarOutlined,
  HistoryOutlined,
  WalletOutlined,
  ShoppingCartOutlined,
} from "@ant-design/icons";

type RegisterLayoutProps = {
  title?: string;
  displayName: string;
  picture: string;
  redCoins: number;
  whiteCoins: number;
};

export function RegisterLayout({
  title,
  displayName,
  picture,
  redCoins,
  whiteCoins,
}: RegisterLayoutProps) {
  const { t } = useTranslation();

  const menuItems: LayoutMenuItem[] = [
    {
      key: WEB_URL.home,
      label: t("page.home.title"),
      icon: <HomeOutlined />,
    },
    {
      key: "",
      label: t("page.tarot.parentTitle"),
      icon: <StarOutlined />,
      children: [
        {
          key: WEB_URL.tarot,
          label: t("page.tarot.title"),
        },
        {
          key: WEB_URL.aiTarot,
          label: t("page.aiTarot.title"),
        },
        {
          key: "deepTarot",
          label: t("page.aiDeepTarot.parentTitle"),
          children: [
            {
              key: WEB_URL.aiDeepTarotTwelveHouses,
              label: t("page.aiDeepTarot.spreads.twelveHouses.title"),
            },
            {
              key: WEB_URL.aiDeepTarotTwelveMonths,
              label: t("page.aiDeepTarot.spreads.twelveMonths.title"),
            },
            {
              key: WEB_URL.aiDeepTarotCrossroads,
              label: t("page.aiDeepTarot.spreads.crossroads.title"),
            },
          ],
        },
      ],
    },
    {
      key: "history",
      label: t("page.history.parentTitle"),
      icon: <HistoryOutlined />,
      children: [
        {
          key: WEB_URL.tarotHistory,
          label: t("page.history.title"),
        },
        {
          key: WEB_URL.aiTarotHistory,
          label: t("page.historyAiTarot.title"),
        },
        {
          key: WEB_URL.aiDeepTarotHistory,
          label: t("page.historyAiDeepTarot.title"),
        },
      ],
    },
    {
      key: WEB_URL.library,
      label: t("page.library.title"),
      icon: <BookOutlined />,
    },
    {
      key: WEB_URL.wallet,
      label: t("page.wallet.title"),
      icon: <WalletOutlined />,
    },

    {
      key: WEB_URL.shop,
      label: t("page.shop.title"),
      icon: <ShoppingCartOutlined />,
    },
  ];

  return (
    <BaseLayout
      variant="register"
      title={title ? t(title) : t("page.home.title")}
      items={menuItems}
      displayName={displayName}
      picture={picture}
      redCoins={redCoins}
      whiteCoins={whiteCoins}
    />
  );
}
