import { useEffect } from "react";
import { App, Col, Form, Input, InputNumber, Row, Switch } from "antd";
import { useTranslation } from "react-i18next";
import { ResponsiveModal } from "@/components";
import { useCreateShopPackage, useUpdateShopPackage } from "@/hooks/api";
import type {
  CreateShopPackageRequest,
  GetAllShopPackagesItem,
  UpdateShopPackageRequest,
} from "@/types";
import { getErrorMessage, getFormFieldErrors } from "@/utils";

export interface ShopPackageFormValues {
  code: string;
  redCoins: number;
  whiteCoins: number;
  oldPrice: number;
  price: number;
  isEnabled: boolean;
}

const DEFAULT_VALUES: ShopPackageFormValues = {
  code: "",
  redCoins: 0,
  whiteCoins: 0,
  oldPrice: 0,
  price: 0,
  isEnabled: true,
};

export interface ShopPackageFormModalProps {
  open: boolean;
  packageItem: GetAllShopPackagesItem | null;
  onClose: () => void;
}

/**
 * Create/edit form for a shop package. The package is null when creating a new
 * one, otherwise the selected package is used to prefill the form.
 */
export default function ShopPackageFormModal({
  open,
  packageItem,
  onClose,
}: ShopPackageFormModalProps) {
  const { t } = useTranslation();
  const { message } = App.useApp();
  const [form] = Form.useForm<ShopPackageFormValues>();
  const { mutateAsync: createPackage, isPending: isCreating } =
    useCreateShopPackage();
  const { mutateAsync: updatePackage, isPending: isUpdating } =
    useUpdateShopPackage();

  const isEdit = packageItem !== null;
  const loading = isCreating || isUpdating;

  useEffect(() => {
    if (!open) {
      return;
    }

    if (packageItem) {
      form.setFieldsValue({
        code: packageItem.code,
        redCoins: packageItem.redCoins,
        whiteCoins: packageItem.whiteCoins,
        oldPrice: packageItem.oldPrice,
        price: packageItem.price,
        isEnabled: packageItem.isEnabled,
      });
    } else {
      form.resetFields();
    }
  }, [open, packageItem, form]);

  const handleSubmit = async () => {
    let values: ShopPackageFormValues;

    try {
      values = await form.validateFields();
    } catch {
      return;
    }

    const normalizedValues: ShopPackageFormValues = {
      ...values,
      code: values.code.trim().toLowerCase(),
    };

    try {
      if (packageItem) {
        const request: UpdateShopPackageRequest = normalizedValues;
        await updatePackage({ id: packageItem.id, request });
        message.success(t("page.admin.shopPackages.updateSuccess"));
      } else {
        const request: CreateShopPackageRequest = normalizedValues;
        await createPackage(request);
        message.success(t("page.admin.shopPackages.createSuccess"));
      }

      onClose();
    } catch (error) {
      const fieldErrors = getFormFieldErrors<ShopPackageFormValues>(error);

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
          ? t("page.admin.shopPackages.form.editTitle")
          : t("page.admin.shopPackages.form.createTitle")
      }
      onClose={onClose}
      loading={loading}
      actions={[
        {
          buttonType: "ok",
          handle: () => void handleSubmit(),
          label: t("page.admin.shopPackages.form.saveButton"),
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
          label={t("page.admin.shopPackages.form.code")}
          normalize={(value?: string) => (value ?? "").toLowerCase()}
          rules={[
            {
              required: true,
              message: t("page.admin.shopPackages.form.codeRequired"),
            },
            {
              max: 50,
              message: t("page.admin.shopPackages.form.codeMax"),
            },
          ]}
        >
          <Input maxLength={50} />
        </Form.Item>

        <Row gutter={16}>
          <Col xs={24} sm={12}>
            <Form.Item
              name="redCoins"
              label={t("page.admin.shopPackages.form.redCoins")}
              rules={[
                {
                  required: true,
                  message: t("page.admin.shopPackages.form.required"),
                },
                {
                  type: "number",
                  min: 0,
                  message: t("page.admin.shopPackages.form.nonNegative"),
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
          <Col xs={24} sm={12}>
            <Form.Item
              name="whiteCoins"
              label={t("page.admin.shopPackages.form.whiteCoins")}
              rules={[
                {
                  required: true,
                  message: t("page.admin.shopPackages.form.required"),
                },
                {
                  type: "number",
                  min: 0,
                  message: t("page.admin.shopPackages.form.nonNegative"),
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
              name="oldPrice"
              label={t("page.admin.shopPackages.form.oldPrice")}
              rules={[
                {
                  required: true,
                  message: t("page.admin.shopPackages.form.required"),
                },
                {
                  type: "number",
                  min: 0,
                  message: t("page.admin.shopPackages.form.nonNegative"),
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
          <Col xs={24} sm={12}>
            <Form.Item
              name="price"
              label={t("page.admin.shopPackages.form.price")}
              rules={[
                {
                  required: true,
                  message: t("page.admin.shopPackages.form.required"),
                },
                {
                  type: "number",
                  min: 1,
                  message: t("page.admin.shopPackages.form.pricePositive"),
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
        </Row>

        <Form.Item
          name="isEnabled"
          label={t("page.admin.shopPackages.form.isEnabled")}
          valuePropName="checked"
        >
          <Switch />
        </Form.Item>
      </Form>
    </ResponsiveModal>
  );
}
