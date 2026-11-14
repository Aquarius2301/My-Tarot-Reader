import {
  TwelveHousesDrawCardsStep,
  TwelveHousesIntroStep,
} from "./components";
import { useGetCurrentUser } from "@/hooks/api";
import { Typography } from "antd";
import { useState } from "react";
import { useTranslation } from "react-i18next";

const { Title } = Typography;

/** Two-step 12 houses deep tarot reading: introduce the spread, then draw the cards. */
export default function TwelveHousesPage() {
  const { t } = useTranslation();
  const { data: user } = useGetCurrentUser();
  const [step, setStep] = useState<"intro" | "draw">("intro");

  return (
    <div style={{ maxWidth: 960, margin: "0 auto" }}>
      <Title level={2} style={{ textAlign: "center" }}>
        {t("page.aiDeepTarot.spreads.twelveHouses.title")}
      </Title>

      {step === "intro" ? (
        <TwelveHousesIntroStep
          balance={user?.redCoin ?? 0}
          onNext={() => setStep("draw")}
        />
      ) : (
        <TwelveHousesDrawCardsStep onBack={() => setStep("intro")} />
      )}
    </div>
  );
}
