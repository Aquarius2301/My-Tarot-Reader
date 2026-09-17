import React from "react";
import { Modal, Drawer, Button, Grid, Space } from "antd";
import { useTranslation } from "react-i18next";

const { useBreakpoint } = Grid;

export type ActionButtonType = "cancel" | "ok" | "delete";

export interface ResponsiveModalAction {
  buttonType: ActionButtonType;
  handle: () => void;
  disabled?: boolean;
  label?: React.ReactNode;
}

export type ResponsiveModalSize = "sm" | "md" | "lg" | "xl";

export interface ResponsiveModalProps {
  open?: boolean;
  onClose?: () => void;
  title?: React.ReactNode;
  children?: React.ReactNode;
  actions?: ResponsiveModalAction[];
  loading?: boolean;
  size?: ResponsiveModalSize;
}

// Độ rộng tối đa cho content theo size (áp dụng cho Modal ở web)
const SIZE_WIDTH_MAP: Record<ResponsiveModalSize, number> = {
  sm: 400,
  md: 600,
  lg: 800,
  xl: 1000,
};

const DEFAULT_SIZE: ResponsiveModalSize = "md";

// Map buttonType -> danger/type mặc định của Antd Button
const BUTTON_TYPE_CONFIG: Record<
  ActionButtonType,
  { type: "primary" | "default"; danger?: boolean }
> = {
  ok: { type: "primary" },
  delete: { type: "primary", danger: true },
  cancel: { type: "default" },
};

const ResponsiveModal: React.FC<ResponsiveModalProps> = ({
  open,
  onClose,
  title,
  children,
  actions,
  loading,
  size = DEFAULT_SIZE,
}) => {
  const { t } = useTranslation();
  const screens = useBreakpoint();
  const isMobile = !screens.md; // < md thì coi là mobile -> dùng Drawer

  const width = SIZE_WIDTH_MAP[size] ?? SIZE_WIDTH_MAP[DEFAULT_SIZE];

  const handleClose = () => {
    if (loading) return;
    onClose?.();
  };

  const renderFooter = () => {
    if (!actions || actions.length === 0) return null;

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
  };

  if (isMobile) {
    return (
      <Drawer
        open={open}
        onClose={handleClose}
        title={title}
        placement="bottom"
        closable={!loading}
        mask={{ closable: !loading }}
        keyboard={!loading}
        footer={renderFooter()}
        size="auto"
      >
        {children}
      </Drawer>
    );
  }

  return (
    <Modal
      open={open}
      onCancel={handleClose}
      title={title}
      width={width}
      closable={!loading}
      mask={{ closable: !loading }}
      keyboard={!loading}
      footer={renderFooter()}
      destroyOnHidden
    >
      {children}
    </Modal>
  );
};

export default ResponsiveModal;
