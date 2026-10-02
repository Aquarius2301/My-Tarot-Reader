import { ResponsiveModal } from "@/components";
import { useTranslation } from "react-i18next";

interface DeleteAiDeepTarotModalProps {
  open: boolean;
  loading?: boolean;
  onClose: () => void;
  onConfirm: () => void;
}

export default function DeleteAiDeepTarotModal({
  open,
  loading = false,
  onClose,
  onConfirm,
}: DeleteAiDeepTarotModalProps) {
  const { t } = useTranslation();

  return (
    <ResponsiveModal
      title={t("page.historyAiDeepTarot.deleteTitle")}
      open={open}
      onClose={onClose}
      actions={[
        {
          buttonType: "delete",
          label: t("page.historyAiDeepTarot.deleteConfirm"),
          handle: onConfirm,
          disabled: loading,
        },
      ]}
    >
      {t("page.historyAiDeepTarot.deleteDescription")}
    </ResponsiveModal>
  );
}
