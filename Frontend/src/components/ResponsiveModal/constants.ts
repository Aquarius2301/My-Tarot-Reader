import type { ActionButtonType, ResponsiveModalSize } from "./types";

// Max width of modal for each size
export const SIZE_WIDTH_MAP: Record<ResponsiveModalSize, number> = {
  sm: 400,
  md: 600,
  lg: 800,
  xl: 1000,
};

export const DEFAULT_SIZE: ResponsiveModalSize = "md";

// Map buttonType -> antd button props
export const BUTTON_TYPE_CONFIG: Record<
  ActionButtonType,
  { type: "primary" | "default"; danger?: boolean }
> = {
  ok: { type: "primary" },
  delete: { type: "primary", danger: true },
  cancel: { type: "default" },
};
