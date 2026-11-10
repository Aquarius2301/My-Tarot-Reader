import { Button, Space } from "antd";
import { useTranslation } from "react-i18next";
import { BUTTON_TYPE_CONFIG } from "./constants";
import type { ResponsiveModalAction } from "./types";

type ModalFooterProps = {
  actions: ResponsiveModalAction[];
  loading?: boolean;
};

export function ModalFooter({ actions, loading }: ModalFooterProps) {
  const { t } = useTranslation();

  return (
    <Space style={{ width: "100%", justifyContent: "flex-end" }} wrap>
      {actions.map((action, index) => {
        const config = BUTTON_TYPE_CONFIG[action.buttonType];

        return (
          <Button
            key={`${action.buttonType}-${index}`}
            type={config.type}
            danger={config.danger}
            disabled={action.disabled || loading}
            loading={loading}
            onClick={action.handle}
          >
            {action.label ?? t(`component.modal.${action.buttonType}`)}
          </Button>
        );
      })}
    </Space>
  );
}
