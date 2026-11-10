import type React from "react";

export type ActionButtonType = "cancel" | "ok" | "delete";

export type ResponsiveModalSize = "sm" | "md" | "lg" | "xl";

export interface ResponsiveModalAction {
  buttonType: ActionButtonType;
  handle: () => void;
  disabled?: boolean;
  label?: React.ReactNode;
}

export interface ResponsiveModalProps {
  open?: boolean;
  onClose?: () => void;
  title?: React.ReactNode;
  children?: React.ReactNode;
  actions?: ResponsiveModalAction[];
  loading?: boolean;
  size?: ResponsiveModalSize;
}
