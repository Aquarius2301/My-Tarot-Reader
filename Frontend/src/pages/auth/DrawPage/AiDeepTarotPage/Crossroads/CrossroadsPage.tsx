import type { CrossroadsTimeFrame } from "@/constants";
import { useState } from "react";
import { Typography } from "antd";
import { useTranslation } from "react-i18next";
import {
  CrossroadsDrawCardsStep,
  CrossroadsQuestionStep,
  type CrossroadsReadingInput,
} from "./components";

const { Title } = Typography;

/** A valid form payload collected by the question step, before the cards are drawn. */
const EMPTY_INPUT: CrossroadsReadingInput = {
  question: "",
  options: [],
};

/**
 * Three-step crossroads deep tarot reading: describe the decision, then draw the
 * comparison spread (3 cards per option plus one closing card).
 */
export default function CrossroadsPage() {
  const { t } = useTranslation();
  const [step, setStep] = useState<"question" | "draw">("question");
  const [input, setInput] = useState<CrossroadsReadingInput>(EMPTY_INPUT);

  const handleQuestionSubmit = (values: {
    question: string;
    options: string[];
    timeFrame?: CrossroadsTimeFrame;
  }) => {
    setInput(values);
    setStep("draw");
  };

  return (
    <div style={{ maxWidth: 960, margin: "0 auto" }}>
      <Title level={2} style={{ textAlign: "center" }}>
        {t("page.aiDeepTarot.spreads.crossroads.title")}
      </Title>

      {step === "question" ? (
        <CrossroadsQuestionStep onNext={handleQuestionSubmit} />
      ) : (
        <CrossroadsDrawCardsStep
          input={input}
          onBack={() => setStep("question")}
        />
      )}
    </div>
  );
}
