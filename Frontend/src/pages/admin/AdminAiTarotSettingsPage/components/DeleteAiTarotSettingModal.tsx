import { ResponsiveModal } from "@/components";
import { useTranslation } from "react-i18next";

export interface DeleteAiTarotSettingModalProps {
  open: boolean;
  title: string;
  description: string;
  loading?: boolean;
  onClose: () => void;
  onConfirm: () => void;
}

/** Confirmation modal shared by the AI tarot setting lists. */
export default function DeleteAiTarotSettingModal({
  open,
  title,
  description,
  loading = false,
  onClose,
  onConfirm,
}: DeleteAiTarotSettingModalProps) {
  const { t } = useTranslation();

  return (
    <ResponsiveModal
      title={title}
      open={open}
      onClose={onClose}
      loading={loading}
      actions={[
        {
          buttonType: "delete",
          handle: onConfirm,
          disabled: loading,
          label: t("page.admin.aiTarotSettings.delete.confirm"),
        },
      ]}
      size="sm"
    >
      {description}
    </ResponsiveModal>
  );
}
