import { Typography, theme } from "antd";

const { Text } = Typography;

export type StreakStatProps = {
  label: string;
  value: number;
  unit: string;
};

export function StreakStat({ label, value, unit }: StreakStatProps) {
  const { token } = theme.useToken();

  return (
    <div style={{ textAlign: "center" }}>
      <Text
        style={{
          display: "block",
          fontSize: 24,
          fontWeight: 700,
          color: token.colorPrimary,
          lineHeight: 1.2,
        }}
      >
        {value} {unit}
      </Text>
      <Text type="secondary" style={{ fontSize: 12 }}>
        {label}
      </Text>
    </div>
  );
}
