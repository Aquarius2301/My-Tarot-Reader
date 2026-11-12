import {
  GuestHomePage,
  GuestTarotPage,
  LoginCallBackPage,
  LoginPage,
} from "@/pages/guest";
import {
  HomePage,
  LibraryPage,
  TarotPage,
  AiDeepTarotResultPage,
  AiTarotPage,
  AiTarotResultPage,
  HistoryAiDeepTarotPage,
  HistoryAiTarotPage,
  HistoryTarotPage,
  ShopCheckoutPage,
  ShopPage,
  WalletPage,
  TwelveHousesPage,
  TwelveMonthsPage,
  CrossroadsPage,
} from "@/pages/register";

export const WEB_URL = {
  guestHome: "/guest",
  register: "/register",
  login: "/login",
  loginCallback: "/login/callback",
  guestTarot: "/draw/guest",

  home: "/",
  tarot: "/draw",
  aiTarot: "/draw/ai",
  aiTarotResult: "/result/ai",
  aiDeepTarotTwelveHouses: "/draw/deep/twelve-houses",
  aiDeepTarotTwelveMonths: "/draw/deep/twelve-months",
  aiDeepTarotCrossroads: "/draw/deep/crossroads",
  aiDeepTarotResult: "/result/deep",
  tarotHistory: "/history",
  aiTarotHistory: "/history/ai",
  aiDeepTarotHistory: "/history/deep",
  library: "/library",
  wallet: "/wallet",
  shop: "/shop",
  shopCheckout: "/shop/checkout",

  admin: "/admin",
  adminStatistics: "/admin/statistics",
  adminShopPackages: "/admin/shop-packages",
  adminAiTarotSettings: "/admin/ai-tarot-settings",
} as const;

export interface AppRoute {
  titleKey: string;
  path: string;
  component: React.ComponentType;
}

export const publicRoutes: AppRoute[] = [
  {
    titleKey: "page.home.title",
    path: WEB_URL.guestHome,
    component: GuestHomePage,
  },
  {
    titleKey: "page.login.title",
    path: WEB_URL.login,
    component: LoginPage,
  },
  {
    titleKey: "page.tarot.title",
    path: WEB_URL.guestTarot,
    component: GuestTarotPage,
  },
  {
    titleKey: "page.login.title",
    path: WEB_URL.loginCallback,
    component: LoginCallBackPage,
  },
];

export const registerRoutes: AppRoute[] = [
  {
    titleKey: "page.home.title",
    path: WEB_URL.home,
    component: HomePage,
  },
  {
    titleKey: "page.library.title",
    path: WEB_URL.library,
    component: LibraryPage,
  },
  {
    titleKey: "page.tarot.title",
    path: WEB_URL.tarot,
    component: TarotPage,
  },
  {
    titleKey: "page.aiTarot.title",
    path: WEB_URL.aiTarot,
    component: AiTarotPage,
  },
  {
    titleKey: "page.aiDeepTarot.title",
    path: WEB_URL.aiDeepTarotTwelveHouses,
    component: TwelveHousesPage,
  },
  {
    titleKey: "page.aiDeepTarot.title",
    path: WEB_URL.aiDeepTarotTwelveMonths,
    component: TwelveMonthsPage,
  },
  {
    titleKey: "page.aiDeepTarot.title",
    path: WEB_URL.aiDeepTarotCrossroads,
    component: CrossroadsPage,
  },
  {
    titleKey: "page.history.title",
    path: WEB_URL.tarotHistory,
    component: HistoryTarotPage,
  },
  {
    titleKey: "page.historyAiTarot.title",
    path: WEB_URL.aiTarotHistory,
    component: HistoryAiTarotPage,
  },
  {
    titleKey: "page.historyAiDeepTarot.title",
    path: WEB_URL.aiDeepTarotHistory,
    component: HistoryAiDeepTarotPage,
  },
  {
    titleKey: "page.aiTarot.result.title",
    path: WEB_URL.aiTarotResult + "/:readingId",
    component: AiTarotResultPage,
  },
  {
    titleKey: "page.aiDeepTarot.result.title",
    path: WEB_URL.aiDeepTarotResult + "/:readingId",
    component: AiDeepTarotResultPage,
  },
  {
    titleKey: "page.wallet.title",
    path: WEB_URL.wallet,
    component: WalletPage,
  },
  {
    titleKey: "page.shop.title",
    path: WEB_URL.shop,
    component: ShopPage,
  },
  {
    titleKey: "page.shop.checkoutTitle",
    path: WEB_URL.shopCheckout + "/:orderId",
    component: ShopCheckoutPage,
  },
];
