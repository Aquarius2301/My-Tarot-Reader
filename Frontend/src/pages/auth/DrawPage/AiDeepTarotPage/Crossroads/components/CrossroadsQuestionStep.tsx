import {
  CROSSROADS_MAX_OPTIONS,
  CROSSROADS_MIN_OPTIONS,
  CROSSROADS_OPTION_MAX_LENGTH,
  CROSSROADS_QUESTION_MAX_LENGTH,
  CROSSROADS_TIME_FRAMES,
  getCrossroadsCardCount,
  getCrossroadsCost,
  type CrossroadsTimeFrame,
} from "@/constants";
import { useGetCurrentUser } from "@/hooks/api";
import {
  Button,
  Card,
  Flex,
  Form,
  Input,
  Select,
  Typography,
  theme,
} from "antd";
import { DeleteOutlined, PlusOutlined, ThunderboltFilled, WalletFilled } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import CrossroadsGuide from "./CrossroadsGuide";

const { Text, Paragraph } = Typography;

export interface CrossroadsQuestionStepValues {
  question: string;
  options: string[];
  timeFrame?: CrossroadsTimeFrame;
}

export interface CrossroadsQuestionStepProps {
  onNext: (values: CrossroadsQuestionStepValues) => void;
}

/**
 * Step 1: collect the decision, the options being compared and the timeframe.
 *
 * The option count is what sizes the spread, so it is validated here (client
 * side) and again by the backend on submit. The red coin cost is one per
 * option, so it is shown live as the user adds and removes options.
 */
export default function CrossroadsQuestionStep({
  onNext,
}: CrossroadsQuestionStepProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const { data: user } = useGetCurrentUser();
  const [form] = Form.useForm<CrossroadsQuestionStepValues>();

  const optionCount = Form.useWatch("options", form)?.length ?? CROSSROADS_MIN_OPTIONS;
  const isOptionCountValid =
    optionCount >= CROSSROADS_MIN_OPTIONS && optionCount <= CROSSROADS_MAX_OPTIONS;

  const cardCount = getCrossroadsCardCount(optionCount);
  const cost = getCrossroadsCost(optionCount);
  const balance = user?.redCoin ?? 0;
  const canAfford = balance >= cost;

  const handleFinish = (values: CrossroadsQuestionStepValues) => {
    onNext({
      question: values.question.trim(),
      options: values.options.map((option) => option.trim()),
      timeFrame: values.timeFrame,
    });
  };

  return (
    <Form<CrossroadsQuestionStepValues>
      form={form}
      layout="vertical"
      requiredMark={false}
      initialValues={{
        question: "",
        options: ["", ""],
        timeFrame: undefined,
      }}
      onFinish={handleFinish}
    >
      <Flex vertical gap={token.marginLG}>
        <Text
          type="secondary"
          style={{ display: "block", textAlign: "center" }}
        >
          {t("page.aiDeepTarot.spreads.crossroads.subtitle")}
        </Text>

        <CrossroadsGuide />

        <Card>
          <Flex vertical gap={token.marginSM}>
            <Text strong>
              {t("page.aiDeepTarot.spreads.crossroads.options.title")}
            </Text>
            <Paragraph type="secondary" style={{ margin: 0 }}>
              {t("page.aiDeepTarot.spreads.crossroads.options.hint")}
            </Paragraph>

            <Form.List name="options">
              {(fields, { add, remove }, { errors }) => (
                <Flex vertical gap={token.marginSM}>
                  {fields.map((field, index) => (
                    <Form.Item
                      key={field.key}
                      name={field.name}
                      rules={[
                        {
                          required: true,
                          whitespace: true,
                          message: t(
                            "page.aiDeepTarot.spreads.crossroads.errors.emptyOption",
                          ),
                        },
                        {
                          max: CROSSROADS_OPTION_MAX_LENGTH,
                          message: t(
                            "page.aiDeepTarot.spreads.crossroads.errors.optionTooLong",
                          ),
                        },
                      ]}
                      style={{ marginBottom: 0 }}
                    >
                      <Input
                        placeholder={t(
                          "page.aiDeepTarot.spreads.crossroads.options.placeholder",
                        )}
                        maxLength={CROSSROADS_OPTION_MAX_LENGTH}
                        addonBefore={t(
                          "page.aiDeepTarot.spreads.crossroads.options.label",
                          { index: index + 1 },
                        )}
                        suffix={
                          fields.length > CROSSROADS_MIN_OPTIONS ? (
                            <Button
                              type="text"
                              size="small"
                              danger
                              icon={<DeleteOutlined />}
                              aria-label={t(
                                "page.aiDeepTarot.spreads.crossroads.options.remove",
                              )}
                              onClick={() => remove(field.name)}
                            />
                          ) : null
                        }
                      />
                    </Form.Item>
                  ))}

                  <Form.ErrorList errors={errors} />

                  <Form.Item
                    noStyle
                    shouldUpdate={(prev, next) =>
                      prev.options?.length !== next.options?.length
                    }
                  >
                    <Button
                      type="dashed"
                      icon={<PlusOutlined />}
                      disabled={fields.length >= CROSSROADS_MAX_OPTIONS}
                      onClick={() => add("")}
                    >
                      {t("page.aiDeepTarot.spreads.crossroads.options.add")}
                    </Button>
                  </Form.Item>
                </Flex>
              )}
            </Form.List>
          </Flex>
        </Card>

        <Card>
          <Flex vertical gap={token.marginSM}>
            <Text strong>
              {t("page.aiDeepTarot.spreads.crossroads.question.title")}
            </Text>
            <Paragraph type="secondary" style={{ margin: 0 }}>
              {t("page.aiDeepTarot.spreads.crossroads.question.hint")}
            </Paragraph>
            <Form.Item
              name="question"
              rules={[
                {
                  required: true,
                  whitespace: true,
                  message: t(
                    "page.aiDeepTarot.spreads.crossroads.errors.questionEmpty",
                  ),
                },
                {
                  max: CROSSROADS_QUESTION_MAX_LENGTH,
                  message: t(
                    "page.aiDeepTarot.spreads.crossroads.errors.questionTooLong",
                  ),
                },
              ]}
              style={{ marginBottom: 0 }}
            >
              <Input.TextArea
                rows={3}
                maxLength={CROSSROADS_QUESTION_MAX_LENGTH}
                showCount
                placeholder={t(
                  "page.aiDeepTarot.spreads.crossroads.question.placeholder",
                )}
              />
            </Form.Item>
          </Flex>
        </Card>

        <Card>
          <Flex vertical gap={token.marginSM}>
            <Text strong>
              {t("page.aiDeepTarot.spreads.crossroads.timeFrame.title")}
            </Text>
            <Paragraph type="secondary" style={{ margin: 0 }}>
              {t("page.aiDeepTarot.spreads.crossroads.timeFrame.hint")}
            </Paragraph>
            <Form.Item name="timeFrame" style={{ marginBottom: 0 }}>
              <Select
                allowClear
                placeholder={t(
                  "page.aiDeepTarot.spreads.crossroads.timeFrame.none",
                )}
                options={CROSSROADS_TIME_FRAMES.map((timeFrame) => ({
                  label: t(
                    `page.aiDeepTarot.spreads.crossroads.timeFrames.${timeFrame}`,
                  ),
                  value: timeFrame,
                }))}
              />
            </Form.Item>
          </Flex>
        </Card>

        <Card>
          <Flex vertical gap={token.marginSM}>
            <Flex justify="space-between" align="center" wrap gap={token.marginSM}>
              <Text>
                <WalletFilled />{" "}
                {t("page.aiDeepTarot.common.balance", { balance })}
              </Text>
              <Text>
                <ThunderboltFilled />{" "}
                {t("page.aiDeepTarot.common.cost", { cost })}
              </Text>
            </Flex>

            <Text type="secondary">
              {t("page.aiDeepTarot.common.cardCount", {
                count: cardCount,
                positions: cardCount,
              })}
            </Text>

            {!canAfford && (
              <Text type="warning">
                {t("page.aiDeepTarot.common.insufficientCoins", { cost, balance })}
              </Text>
            )}

            <Button
              type="primary"
              size="large"
              block
              htmlType="submit"
              disabled={!canAfford || !isOptionCountValid}
            >
              {t("page.aiDeepTarot.common.continue")}
            </Button>
          </Flex>
        </Card>
      </Flex>
    </Form>
  );
}
