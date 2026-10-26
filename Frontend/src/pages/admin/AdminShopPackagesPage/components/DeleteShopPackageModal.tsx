import { ResponsiveModal } from "@/components";
import { useTranslation } from "react-i18next";

export interface DeleteShopPackageModalProps {
  open: boolean;
  code?: string;
  loading?: boolean;
  onClose: () => void;
  onConfirm: () => void;
}

export default function DeleteShopPackageModal({
  open,
  code,
  loading = false,
  onClose,
  onConfirm,
}: DeleteShopPackageModalProps) {
  const { t } = useTranslation();

  return (
    <ResponsiveModal
      title={t("page.admin.shopPackages.delete.title")}
      open={open}
      onClose={onClose}
      loading={loading}
      actions={[
        {
          buttonType: "delete",
          handle: onConfirm,
          disabled: loading,
          label: t("page.admin.shopPackages.delete.confirm"),
        },
      ]}
      size="sm"
    >
      {t("page.admin.shopPackages.delete.description", { code: code ?? "" })}
    </ResponsiveModal>
  );
}
