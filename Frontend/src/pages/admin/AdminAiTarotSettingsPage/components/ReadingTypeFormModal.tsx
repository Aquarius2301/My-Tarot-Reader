import { useEffect } from "react";
import { App, Form, Input, Switch } from "antd";
import { useTranslation } from "react-i18next";
import { ResponsiveModal } from "@/components";
import {
  useCreateAiTarotReadingType,
  useUpdateAiTarotReadingType,
} from "@/hooks/api";
import type {
  AdminAiTarotReadingTypeItem,
  CreateAiTarotReadingTypeRequest,
  UpdateAiTarotReadingTypeRequest,
} from "@/types";
import { getErrorMessage, getFormFieldErrors } from "@/utils";

export interface ReadingTypeFormValues {
  code: string;
  nameEn: string;
  nameVi: string;
  isEnabled: boolean;
}

const DEFAULT_VALUES: ReadingTypeFormValues = {
  code: "",
  nameEn: "",
  nameVi: "",
  isEnabled: true,
};

export interface ReadingTypeFormModalProps {
  open: boolean;
  item: AdminAiTarotReadingTypeItem | null;
  onClose: () => void;
}

/** Create/edit form for a question topic. */
export default function ReadingTypeFormModal({
  open,
  item,
  onClose,
}: ReadingTypeFormModalProps) {
  const { t } = useTranslation();
  const { message } = App.useApp();
  const [form] = Form.useForm<ReadingTypeFormValues>();
  const { mutateAsync: createReadingType, isPending: isCreating } =
    useCreateAiTarotReadingType();
  const { mutateAsync: updateReadingType, isPending: isUpdating } =
    useUpdateAiTarotReadingType();

  const isEdit = item !== null;
  const loading = isCreating || isUpdating;

  useEffect(() => {
    if (!open) {
      return;
    }

    if (item) {
      form.setFieldsValue({
        code: item.code,
        nameEn: item.nameEn,
        nameVi: item.nameVi,
        isEnabled: item.isEnabled,
      });
    } else {
      form.resetFields();
    }
  }, [open, item, form]);

  const handleSubmit = async () => {
    let values: ReadingTypeFormValues;

    try {
      values = await form.validateFields();
    } catch {
      return;
    }

    const normalizedValues: ReadingTypeFormValues = {
      ...values,
      code: values.code.trim().toLowerCase(),
    };

    try {
      if (item) {
        const request: UpdateAiTarotReadingTypeRequest = normalizedValues;
        await updateReadingType({ id: item.id, request });
        message.success(
          t("page.admin.aiTarotSettings.readingTypes.updateSuccess"),
        );
      } else {
        const request: CreateAiTarotReadingTypeRequest = normalizedValues;
        await createReadingType(request);
        message.success(
          t("page.admin.aiTarotSettings.readingTypes.createSuccess"),
        );
      }

      onClose();
    } catch (error) {
      const fieldErrors = getFormFieldErrors<ReadingTypeFormValues>(error);

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
          ? t("page.admin.aiTarotSettings.readingTypes.form.editTitle")
          : t("page.admin.aiTarotSettings.readingTypes.form.createTitle")
      }
      onClose={onClose}
      loading={loading}
      actions={[
        {
          buttonType: "ok",
          handle: () => void handleSubmit(),
          label: t("page.admin.aiTarotSettings.readingTypes.form.saveButton"),
        },
      ]}
      size="md"
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={DEFAULT_VALUES}
        disabled={loading}
      >
        <Form.Item
          name="code"
          label={t("page.admin.aiTarotSettings.readingTypes.form.code")}
          normalize={(value?: string) => (value ?? "").toLowerCase()}
          rules={[
            {
              required: true,
              message: t(
                "page.admin.aiTarotSettings.readingTypes.form.codeRequired",
              ),
            },
            {
              max: 20,
              message: t(
                "page.admin.aiTarotSettings.readingTypes.form.codeMax",
              ),
            },
          ]}
        >
          <Input maxLength={20} />
        </Form.Item>

        <Form.Item
          name="nameEn"
          label={t("page.admin.aiTarotSettings.readingTypes.form.nameEn")}
          rules={[
            {
              required: true,
              message: t(
                "page.admin.aiTarotSettings.readingTypes.form.nameRequired",
              ),
            },
            {
              max: 100,
              message: t(
                "page.admin.aiTarotSettings.readingTypes.form.nameMax",
              ),
            },
          ]}
        >
          <Input maxLength={100} />
        </Form.Item>

        <Form.Item
          name="nameVi"
          label={t("page.admin.aiTarotSettings.readingTypes.form.nameVi")}
          rules={[
            {
              required: true,
              message: t(
                "page.admin.aiTarotSettings.readingTypes.form.nameRequired",
              ),
            },
            {
              max: 100,
              message: t(
                "page.admin.aiTarotSettings.readingTypes.form.nameMax",
              ),
            },
          ]}
        >
          <Input maxLength={100} />
        </Form.Item>

        <Form.Item
          name="isEnabled"
          label={t("page.admin.aiTarotSettings.readingTypes.form.isEnabled")}
          valuePropName="checked"
        >
          <Switch />
        </Form.Item>
      </Form>
    </ResponsiveModal>
  );
}
