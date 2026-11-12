import { Flex, Skeleton } from "antd";
import Text from "antd/es/typography/Text";
import Title from "antd/es/typography/Title";

export interface AppTitleProps {
  title: string;
  subtitle?: string;
  isLoading?: boolean;
}

export function AppTitle({
  title,
  subtitle,
  isLoading = false,
}: AppTitleProps) {
  if (isLoading) {
    return (
      <Flex vertical align="center" gap={16} style={{ margin: 32 }}>
        {/* Title level 2: cao khoảng 40px */}
        <Skeleton.Input
          active
          style={{ width: 240, height: 40, minWidth: 0 }}
        />
        {/* Subtitle: cao khoảng 22px */}
        {subtitle && (
          <Skeleton.Input
            active
            style={{ width: 360, height: 22, minWidth: 0, maxWidth: "100%" }}
          />
        )}
      </Flex>
    );
  }

  return (
    <Flex
      vertical
      align="center"
      gap={16}
      style={{ margin: 32, textAlign: "center" }}
    >
      <Title level={2} style={{ margin: 0 }}>
        {title}
      </Title>
      {subtitle && <Text type="secondary">{subtitle}</Text>}
    </Flex>
  );
}
