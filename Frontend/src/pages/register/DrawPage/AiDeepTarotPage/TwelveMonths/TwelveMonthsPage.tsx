import {
  TwelveMonthsDrawCardsStep,
  TwelveMonthsIntroStep,
} from "./components";
import { useGetCurrentUser } from "@/hooks/api";
import { Typography } from "antd";
import { useState } from "react";
import { useTranslation } from "react-i18next";

const { Title } = Typography;

/** Two-step 12 months deep tarot reading: introduce the spread, then draw the cards. */
export default function TwelveMonthsPage() {
  const { t } = useTranslation();
  const { data: user } = useGetCurrentUser();
  const [step, setStep] = useState<"intro" | "draw">("intro");

  return (
    <div style={{ maxWidth: 960, margin: "0 auto" }}>
      <Title level={2} style={{ textAlign: "center" }}>
        {t("page.aiDeepTarot.spreads.twelveMonths.title")}
      </Title>

      {step === "intro" ? (
        <TwelveMonthsIntroStep
          balance={user?.redCoin ?? 0}
          onNext={() => setStep("draw")}
        />
      ) : (
        <TwelveMonthsDrawCardsStep onBack={() => setStep("intro")} />
      )}
    </div>
  );
}
