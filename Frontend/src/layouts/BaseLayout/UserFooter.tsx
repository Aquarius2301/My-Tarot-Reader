import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Avatar, Button, Tooltip } from "antd";
import { LogoutOutlined, UserOutlined } from "@ant-design/icons";
import Text from "antd/es/typography/Text";

import type { Palette } from "@/constants";

type UserFooterProps = {
  showDetails: boolean;
  displayName?: string;
  picture?: string;
  palette: Palette;
  logoutModalOpen: boolean;
  onLogout: () => void;
};

export function UserFooter({
  showDetails,
  displayName,
  picture,
  palette,
  logoutModalOpen,
  onLogout,
}: UserFooterProps) {
  const { t } = useTranslation();
  const [failedPicture, setFailedPicture] = useState<string | null>(null);
  const [logoutTooltipOpen, setLogoutTooltipOpen] = useState(false);

  const avatarError = Boolean(picture) && failedPicture === picture;

  return (
    <div
      className={`layout-user${showDetails ? "" : " layout-user--collapsed"}`}
      style={{ borderTop: `1px solid ${palette.accentGlow}` }}
    >
      <Tooltip title={showDetails ? undefined : displayName} placement="right">
        <Avatar
          size={showDetails ? 40 : 36}
          src={
            picture && !avatarError ? (
              <img
                src={picture}
                alt={displayName}
                referrerPolicy="no-referrer"
                onError={() => setFailedPicture(picture)}
              />
            ) : undefined
          }
          icon={!picture || avatarError ? <UserOutlined /> : undefined}
          style={{ background: palette.primary, flexShrink: 0 }}
        />
      </Tooltip>

      {showDetails && (
        <div className="layout-user__info">
          <Text className="layout-user__name">{displayName}</Text>
        </div>
      )}

      <Tooltip
        title={t("component.mainLayout.logout")}
        placement={showDetails ? "top" : "right"}
        // When mobile and clicking logoiut, the modal and tooltip will open at the same time, which is not a good UX,
        // so we need to prevent it from opening the tooltip when the modal is open
        open={logoutTooltipOpen && !logoutModalOpen}
        onOpenChange={(open) => setLogoutTooltipOpen(open && !logoutModalOpen)}
      >
        <Button
          type="text"
          danger
          aria-label={t("component.mainLayout.logout")}
          icon={<LogoutOutlined />}
          className="layout-user__logout"
          onClick={() => {
            setLogoutTooltipOpen(false);
            onLogout();
          }}
        />
      </Tooltip>
    </div>
  );
}
