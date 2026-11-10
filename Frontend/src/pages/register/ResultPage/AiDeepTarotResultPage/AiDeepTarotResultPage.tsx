import { AppTitle } from "@/components";
import { useTranslation } from "react-i18next";
import { AiDeepTarotResultItem } from "./AiDeepTarotResultItem/AiDeepTarotResultItem";

/** Shows a created deep tarot reading, whichever spread it belongs to. */
export default function AiDeepTarotResultPage() {
  const { t } = useTranslation();

  return (
    <div>
      <AppTitle title={t("page.aiDeepTarot.result.title")} />

      <AiDeepTarotResultItem />
    </div>
  );
}
