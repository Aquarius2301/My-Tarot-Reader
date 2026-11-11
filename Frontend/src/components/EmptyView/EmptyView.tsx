import { Button, Empty } from "antd";

export interface ButtonProps {
  onClick: () => void;
  children: React.ReactNode;
}

export interface EmptyViewProps {
  description?: string;
  button?: ButtonProps;
}

export function EmptyView({ description, button }: EmptyViewProps) {
  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        minHeight: "100%",
        padding: "40px 16px",
        textAlign: "center",
      }}
    >
      <Empty description={description}>
        {button && (
          <Button type="primary" onClick={button?.onClick}>
            {button?.children}
          </Button>
        )}
      </Empty>
    </div>
  );
}
