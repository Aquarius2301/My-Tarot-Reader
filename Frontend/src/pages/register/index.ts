import { lazy } from "react";

export const TarotPage = lazy(() => import("./DrawPage/TarotPage"));
export const AiTarotPage = lazy(() => import("./DrawPage/AiTarotPage"));
export const CrossroadsPage = lazy(
  () => import("./DrawPage/AiDeepTarotPage/Crossroads"),
);
export const TwelveHousesPage = lazy(
  () => import("./DrawPage/AiDeepTarotPage/TwelveHouses"),
);
export const TwelveMonthsPage = lazy(
  () => import("./DrawPage/AiDeepTarotPage/TwelveMonths"),
);

export const HistoryTarotPage = lazy(
  () => import("./HistoryPage/HistoryTarotPage"),
);
export const HistoryAiTarotPage = lazy(
  () => import("./HistoryPage/HistoryAiTarotPage"),
);
export const HistoryAiDeepTarotPage = lazy(
  () => import("./HistoryPage/HistoryAiDeepTarotPage"),
);

export const HomePage = lazy(() => import("./HomePage"));
export const LibraryPage = lazy(() => import("./LibraryPage"));

export const AiTarotResultPage = lazy(
  () => import("./ResultPage/AiTarotResultPage"),
);
export const AiDeepTarotResultPage = lazy(
  () => import("./ResultPage/AiDeepTarotResultPage"),
);

export const ShopPage = lazy(() => import("./ShopPage"));
export const ShopCheckoutPage = lazy(() => import("./ShopCheckoutPage"));

export const WalletPage = lazy(() => import("./WalletPage"));
