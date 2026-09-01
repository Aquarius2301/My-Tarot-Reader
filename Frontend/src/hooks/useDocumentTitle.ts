import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import { APP_NAME } from "@/constants";

/**
 * Syncs the document title to the given i18n key. Re-runs when the language
 * changes (via the `t` dependency) so the title stays localized.
 */
export const useDocumentTitle = (titleKey?: string) => {
  const { t } = useTranslation();

  useEffect(() => {
    document.title = titleKey ? `${t(titleKey)} | ${APP_NAME}` : APP_NAME;
  }, [titleKey, t]);
};
