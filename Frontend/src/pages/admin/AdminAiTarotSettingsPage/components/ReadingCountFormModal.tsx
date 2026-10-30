import { useEffect } from "react";
import { App, Col, Form, Input, InputNumber, Row, Switch } from "antd";
import { useTranslation } from "react-i18next";
import { ResponsiveModal } from "@/components";
import {
  useCreateAiTarotReadingCount,
  useUpdateAiTarotReadingCount,
} from "@/hooks/api";
import type {
  AdminAiTarotReadingCountItem,
  CreateAiTarotReadingCountRequest,
  UpdateAiTarotReadingCountRequest,
} from "@/types";
import { getErrorMessage, getFormFieldErrors } from "@/utils";

export interface ReadingCountFormValues {
  cardCount: number;
  whiteCoins: number;
  minWords: number;
  maxWords: number;
  positionsEn: string;
  positionsVi: string;
  isEnabled: boolean;
}

const DEFAULT_VALUES: ReadingCountFormValues = {
  cardCount: 3,
  whiteCoins: 0,
  minWords: 300,
  maxWords: 400,
  positionsEn: "",
  positionsVi: "",
  isEnabled: true,
};

const splitPositions = (value: string) =>
  value
    .split(/\r?\n/)
    .map((position) => position.trim())
    .filter(Boolean);

export interface ReadingCountFormModalProps {
  open: boolean;
  item: AdminAiTarotReadingCountItem | null;
  onClose: () => void;
}

/**
 * Create/edit form for a spread configuration. Positions are entered one per
 * line and must match the configured card count.
 */
export default function ReadingCountFormModal({
  open,
  item,
  onClose,
}: ReadingCountFormModalProps) {
  const { t } = useTranslation();
  const { message } = App.useApp();
  const [form] = Form.useForm<ReadingCountFormValues>();
  const { mutateAsync: createReadingCount, isPending: isCreating } =
    useCreateAiTarotReadingCount();
  const { mutateAsync: updateReadingCount, isPending: isUpdating } =
    useUpdateAiTarotReadingCount();

  const isEdit = item !== null;
  const loading = isCreating || isUpdating;

  useEffect(() => {
    if (!open) {
      return;
    }

    if (item) {
      form.setFieldsValue({
        cardCount: item.cardCount,
        whiteCoins: item.whiteCoins,
        minWords: item.minWords,
        maxWords: item.maxWords,
        positionsEn: item.positionsEn.join("\n"),
        positionsVi: item.positionsVi.join("\n"),
        isEnabled: item.isEnabled,
      });
    } else {
      form.resetFields();
    }
  }, [open, item, form]);

  const handleSubmit = async () => {
    let values: ReadingCountFormValues;

    try {
      values = await form.validateFields();
    } catch {
      return;
    }

    const positionsEn = splitPositions(values.positionsEn);
    const positionsVi = splitPositions(values.positionsVi);
    const positionsCountError = t(
      "page.admin.aiTarotSettings.readingCounts.form.positionsCount",
      { cardCount: values.cardCount },
    );

    if (positionsEn.length !== values.cardCount) {
      form.setFields([{ name: "positionsEn", errors: [positionsCountError] }]);
      return;
    }

    if (positionsVi.length !== values.cardCount) {
      form.setFields([{ name: "positionsVi", errors: [positionsCountError] }]);
      return;
    }

    const request = {
      cardCount: values.cardCount,
      whiteCoins: values.whiteCoins,
      positionsEn,
      positionsVi,
      minWords: values.minWords,
      maxWords: values.maxWords,
      isEnabled: values.isEnabled,
    };

    try {
      if (item) {
        const updateRequest: UpdateAiTarotReadingCountRequest = request;
        await updateReadingCount({ id: item.id, request: updateRequest });
        message.success(
          t("page.admin.aiTarotSettings.readingCounts.updateSuccess"),
        );
      } else {
        const createRequest: CreateAiTarotReadingCountRequest = request;
        await createReadingCount(createRequest);
        message.success(
          t("page.admin.aiTarotSettings.readingCounts.createSuccess"),
        );
      }

      onClose();
    } catch (error) {
      const fieldErrors = getFormFieldErrors<ReadingCountFormValues>(error);

      if (fieldErrors.length > 0) {
        form.setFields(fieldErrors);
      } else {
        message.error(getErrorMessage(error));
      }
    }
  };

  return (
    <ResponsiveModal
      open={open}
      title={
        isEdit
          ? t("page.admin.aiTarotSettings.readingCounts.form.editTitle")
          : t("page.admin.aiTarotSettings.readingCounts.form.createTitle")
      }
      onClose={onClose}
      loading={loading}
      actions={[
        {
          buttonType: "ok",
          handle: () => void handleSubmit(),
          label: t("page.admin.aiTarotSettings.readingCounts.form.saveButton"),
        },
      ]}
      size="lg"
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={DEFAULT_VALUES}
        disabled={loading}
      >
        <Row gutter={16}>
          <Col xs={24} sm={12}>
            <Form.Item
              name="cardCount"
              label={t("page.admin.aiTarotSettings.readingCounts.form.cardCount")}
              rules={[
                {
                  required: true,
                  message: t(
                    "page.admin.aiTarotSettings.readingCounts.form.required",
                  ),
                },
                {
                  type: "number",
                  min: 1,
                  message: t(
                    "page.admin.aiTarotSettings.readingCounts.form.cardCountPositive",
                  ),
                },
              ]}
            >
              <InputNumber<number>
                min={1}
                precision={0}
                style={{ width: "100%" }}
              />
            </Form.Item>
          </Col>
          <Col xs={24} sm={12}>
            <Form.Item
              name="whiteCoins"
              label={t(
                "page.admin.aiTarotSettings.readingCounts.form.whiteCoins",
              )}
              rules={[
                {
                  required: true,
                  message: t(
                    "page.admin.aiTarotSettings.readingCounts.form.required",
                  ),
                },
                {
                  type: "number",
                  min: 0,
                  message: t(
                    "page.admin.aiTarotSettings.readingCounts.form.nonNegative",
                  ),
                },
              ]}
            >
              <InputNumber<number>
                min={0}
                precision={0}
                style={{ width: "100%" }}
              />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={16}>
          <Col xs={24} sm={12}>
            <Form.Item
              name="minWords"
              label={t("page.admin.aiTarotSettings.readingCounts.form.minWords")}
              rules={[
                {
                  required: true,
                  message: t(
                    "page.admin.aiTarotSettings.readingCounts.form.required",
                  ),
                },
                {
                  type: "number",
                  min: 1,
                  message: t(
                    "page.admin.aiTarotSettings.readingCounts.form.minWordsPositive",
                  ),
                },
              ]}
            >
              <InputNumber<number>
                min={1}
                precision={0}
                style={{ width: "100%" }}
              />
            </Form.Item>
          </Col>
          <Col xs={24} sm={12}>
            <Form.Item
              name="maxWords"
              label={t("page.admin.aiTarotSettings.readingCounts.form.maxWords")}
              dependencies={["minWords"]}
              rules={[
                {
                  required: true,
                  message: t(
                    "page.admin.aiTarotSettings.readingCounts.form.required",
                  ),
                },
                {
                  type: "number",
                  min: 1,
                  message: t(
                    "page.admin.aiTarotSettings.readingCounts.form.minWordsPositive",
                  ),
                },
                ({ getFieldValue }) => ({
                  validator(_, value) {
                    const minWords = getFieldValue("minWords") as
                      | number
                      | null;

                    if (
                      value == null ||
                      minWords == null ||
                      value >= minWords
                    ) {
                      return Promise.resolve();
                    }

                    return Promise.reject(
                      new Error(
                        t(
                          "page.admin.aiTarotSettings.readingCounts.form.maxWordsMin",
                        ),
                      ),
                    );
                  },
                }),
              ]}
            >
              <InputNumber<number>
                min={1}
                precision={0}
                style={{ width: "100%" }}
              />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item
          name="positionsEn"
          label={t(
            "page.admin.aiTarotSettings.readingCounts.form.positionsEn",
          )}
          extra={t(
            "page.admin.aiTarotSettings.readingCounts.form.positionsHint",
          )}
          rules={[
            {
              required: true,
              message: t(
                "page.admin.aiTarotSettings.readingCounts.form.required",
              ),
            },
          ]}
        >
          <Input.TextArea autoSize={{ minRows: 3, maxRows: 10 }} />
        </Form.Item>

        <Form.Item
          name="positionsVi"
          label={t(
            "page.admin.aiTarotSettings.readingCounts.form.positionsVi",
          )}
          rules={[
            {
              required: true,
              message: t(
                "page.admin.aiTarotSettings.readingCounts.form.required",
              ),
            },
          ]}
        >
          <Input.TextArea autoSize={{ minRows: 3, maxRows: 10 }} />
        </Form.Item>

        <Form.Item
          name="isEnabled"
          label={t("page.admin.aiTarotSettings.readingCounts.form.isEnabled")}
          valuePropName="checked"
        >
          <Switch />
        </Form.Item>
      </Form>
    </ResponsiveModal>
  );
}
