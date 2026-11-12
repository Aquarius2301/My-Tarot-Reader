import type { AppRoute } from "@/routes";
import { useTranslation } from "react-i18next";
import { matchPath, useLocation } from "react-router-dom";

export function useRouteTitle(routes: AppRoute[]) {
  const { pathname } = useLocation();
  const { t } = useTranslation();

  const route = routes.find((r) =>
    matchPath({ path: r.path, end: true }, pathname),
  );

  return route?.titleKey ? t(route.titleKey) : undefined;
}
