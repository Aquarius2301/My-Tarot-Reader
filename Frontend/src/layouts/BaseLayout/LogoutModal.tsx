import { ResponsiveModal } from "@/components";
import { useTranslation } from "react-i18next";

type LogoutModalProps = {
  open: boolean;
  onClose: () => void;
  onLogout: () => void;
  loading: boolean;
};

export function LogoutModal({
  open,
  onClose,
  onLogout,
  loading,
}: LogoutModalProps) {
  const { t } = useTranslation();

  return (
    <ResponsiveModal
      open={open}
      onClose={onClose}
      title={t("component.mainLayout.logout")}
      loading={loading}
      actions={[
        {
          buttonType: "ok",
          handle: onLogout,
          label: t("component.mainLayout.logout"),
        },
      ]}
    >
      {t("component.mainLayout.logoutConfirm")}
    </ResponsiveModal>
  );
}
