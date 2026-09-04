import React from "react";
import { Modal, Drawer, Grid } from "antd";
import type { ModalProps } from "antd/es/modal";
import type { DrawerProps } from "antd/es/drawer";

const { useBreakpoint } = Grid;

export interface ResponsiveModalProps {
  open: boolean;
  onClose: () => void;
  title?: React.ReactNode;
  children?: React.ReactNode;
  footer?: React.ReactNode;
  modalProps?: Omit<ModalProps, "open" | "onCancel" | "title" | "footer">;
  drawerProps?: Omit<DrawerProps, "open" | "onClose" | "title" | "footer">;
  breakpoint?: "xs" | "sm" | "md";
  drawerPlacement?: DrawerProps["placement"];
}

export default function ResponsiveModal({
  open,
  onClose,
  title,
  children,
  footer,
  modalProps,
  drawerProps,
  breakpoint = "sm",
  drawerPlacement = "bottom",
}: ResponsiveModalProps) {
  const screens = useBreakpoint();

  // Define the order of breakpoints from smallest to largest
  const breakpointOrder: Array<"xs" | "sm" | "md" | "lg" | "xl" | "xxl"> = [
    "xs",
    "sm",
    "md",
    "lg",
    "xl",
    "xxl",
  ];
  const thresholdIndex = breakpointOrder.indexOf(breakpoint);
  const isMobile = !breakpointOrder
    .slice(thresholdIndex)
    .some((bp) => screens[bp]);

  if (isMobile) {
    // `drawerProps.styles` may be a plain object OR a render function; resolve
    // the function form so caller styles are preserved on mobile too. We add a
    // height cap so tall content (e.g. a card meaning list) stays inside the
    // viewport and the drawer section/body scroll instead of overflowing past
    // the top on mobile.
    const callerStyles =
      typeof drawerProps?.styles === "function"
        ? drawerProps.styles({ props: drawerProps as DrawerProps })
        : drawerProps?.styles;
    const isVertical =
      drawerPlacement === "bottom" || drawerPlacement === "top";

    return (
      <Drawer
        open={open}
        onClose={onClose}
        title={title}
        placement={drawerPlacement}
        size={isVertical ? "auto" : undefined}
        footer={footer}
        destroyOnHidden
        {...drawerProps}
        styles={{
          ...callerStyles,
          section: {
            maxHeight: isVertical ? "85vh" : undefined,
            ...callerStyles?.section,
          },
        }}
      >
        {children}
      </Drawer>
    );
  }

  return (
    <Modal
      open={open}
      onCancel={onClose}
      title={title}
      footer={footer}
      destroyOnHidden
      {...modalProps}
    >
      {children}
    </Modal>
  );
}
