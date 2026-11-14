import type { GetAiTarotSettingResult } from "@/types";
import { SelectReadingStepLoading } from "./SelectReadingStepLoading";
import { SelectReadingStepForm } from "./StepReadingForm";

interface SelectReadingStepProps {
  settings?: GetAiTarotSettingResult;
  isLoading?: boolean;
  balance: number;
  onNext: (cardCount: number, questionType: string) => void;
}

/** Step 1: pick the spread size and question topic before drawing cards. */
export function SelectReadingStep({
  settings,
  isLoading = false,
  balance,
  onNext,
}: SelectReadingStepProps) {
  if (isLoading) {
    return <SelectReadingStepLoading />;
  }

  if (!settings) {
    return null;
  }

  return (
    <SelectReadingStepForm
      settings={settings}
      balance={balance}
      onNext={onNext}
    />
  );
}
