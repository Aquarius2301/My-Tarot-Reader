import { ResponsiveModal } from "@/components";
import { useTranslation } from "react-i18next";

interface DeleteReadingModalProps {
  open: boolean;
  loading?: boolean;
  onClose: () => void;
  onConfirm: () => void;
}

export default function DeleteReadingModal({
  open,
  loading = false,
  onClose,
  onConfirm,
}: DeleteReadingModalProps) {
  const { t } = useTranslation();

  return (
    <ResponsiveModal
      title={t("page.history.deleteTitle")}
      open={open}
      onClose={onClose}
      actions={[
        {
          buttonType: "delete",
          handle: onConfirm,
          disabled: loading,
        },
      ]}
    >
      {t("page.history.deleteDescription")}
    </ResponsiveModal>
  );
}
