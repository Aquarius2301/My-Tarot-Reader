import { AppTitle } from "@/components";
import { useTranslation } from "react-i18next";
import { HistoryAiDeepList } from "./HistoryAiDeepList/HistoryAiDeepList";

/** Lists all of the user's deep tarot readings; clicking one opens the full result. */
export default function HistoryAiDeepTarotPage() {
  const { t } = useTranslation();

  return (
    <div>
      <AppTitle
        title={t("page.historyAiDeepTarot.title")}
        subtitle={t("page.historyAiDeepTarot.subtitle")}
      />

      <HistoryAiDeepList />
    </div>
  );
}
