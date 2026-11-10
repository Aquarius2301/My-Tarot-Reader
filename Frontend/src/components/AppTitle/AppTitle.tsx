import { Flex } from "antd";
import Text from "antd/es/typography/Text";
import Title from "antd/es/typography/Title";

export interface AppTitleProps {
  title: string;
  subtitle?: string;
}

export function AppTitle({ title, subtitle }: AppTitleProps) {
  return (
    <Flex vertical gap={16} style={{ textAlign: "center", margin: 32 }}>
      <Title level={2} style={{ margin: 0 }}>
        {title}
      </Title>
      <Text style={{}} type="secondary">
        {subtitle}
      </Text>
    </Flex>
  );
}
