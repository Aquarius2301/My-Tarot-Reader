import { ResponsiveModal } from "@/components";
import { useTranslation } from "react-i18next";

interface DeleteAiTarotModalProps {
  open: boolean;
  loading?: boolean;
  onClose: () => void;
  onConfirm: () => void;
}

export default function DeleteAiTarotModal({
  open,
  loading = false,
  onClose,
  onConfirm,
}: DeleteAiTarotModalProps) {
  const { t } = useTranslation();

  return (
    <ResponsiveModal
      title={t("page.historyAiTarot.deleteTitle")}
      open={open}
      onClose={onClose}
      actions={[
        {
          buttonType: "delete",
          label: t("page.historyAiTarot.deleteConfirm"),
          handle: onConfirm,
          disabled: loading,
        },
      ]}
    >
      {t("page.historyAiTarot.deleteDescription")}
    </ResponsiveModal>
  );
}