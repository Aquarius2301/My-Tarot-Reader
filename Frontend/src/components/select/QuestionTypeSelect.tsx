import { QUESTION_TYPES, type QuestionType } from "@/constants";
import { Flex, Select } from "antd";
import { useTranslation } from "react-i18next";

export interface QuestionTypeSelectProps {
  questionType: QuestionType;
  onQuestionTypeChange: (questionType: QuestionType) => void;
}

export default function QuestionTypeSelect({
  questionType,
  onQuestionTypeChange,
}: QuestionTypeSelectProps) {
  const { t } = useTranslation();

  const options = QUESTION_TYPES.map((value) => ({
    value,
    label: t(`select.questionType.${value}`),
  }));

  return (
    <Flex vertical gap={4} style={{ flex: 1, minWidth: 180 }}>
      <label htmlFor="ai-draw-question-type" style={{ fontWeight: 600 }}>
        {t("select.questionTypeLabel")}
      </label>
      <Select<QuestionType>
        id="ai-draw-question-type"
        value={questionType}
        style={{ width: "100%" }}
        options={options}
        onChange={onQuestionTypeChange}
      />
    </Flex>
  );
}
