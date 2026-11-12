import { Modal, Drawer, Grid } from "antd";
import { DEFAULT_SIZE, SIZE_WIDTH_MAP } from "./constants";
import { ModalFooter } from "./ModalFooter";
import type { ResponsiveModalProps } from "./types";

const { useBreakpoint } = Grid;

export function ResponsiveModal({
  open,
  onClose,
  title,
  children,
  actions,
  loading,
  size = DEFAULT_SIZE,
}: ResponsiveModalProps) {
  const screens = useBreakpoint();
  const isMobile = !screens.md; // < 768px is considered mobile

  const width = SIZE_WIDTH_MAP[size] ?? SIZE_WIDTH_MAP[DEFAULT_SIZE];

  const handleClose = () => {
    if (loading) return;
    onClose?.();
  };

  const footer = actions?.length ? (
    <ModalFooter actions={actions} loading={loading} />
  ) : null;

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
        footer={footer}
        size="auto"
        destroyOnHidden
        style={{ maxHeight: "calc(100dvh - 32px)" }}
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
      footer={footer}
      destroyOnHidden
    >
      {children}
    </Modal>
  );
}
