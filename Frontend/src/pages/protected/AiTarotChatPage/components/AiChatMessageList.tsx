import { useEffect, useRef } from "react";
import { Spin, Typography } from "antd";
import { useTranslation } from "react-i18next";
import AiChatMessage from "./AiChatMessage";
import type { AiChatMessage as AiChatMessageType } from "@/types";

const { Text } = Typography;

interface AiChatMessageListProps {
  /** The list of chat messages to display. */
  messages: AiChatMessageType[];
  /** Whether the AI is currently generating a response. */
  isLoading?: boolean;
}

/**
 * Scrollable list of chat messages with auto-scroll behavior
 * and a typing indicator when the AI is thinking.
 */
export default function AiChatMessageList({
  messages,
  isLoading = false,
}: AiChatMessageListProps) {
  const { t } = useTranslation();
  const bottomRef = useRef<HTMLDivElement>(null);

  // Auto-scroll to the bottom when new messages arrive or loading state changes.
  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages.length, isLoading]);

  return (
    <div
      style={{
        flex: 1,
        overflowY: "auto",
        padding: "16px 0",
      }}
    >
      {messages.map((msg, index) => (
        <AiChatMessage key={index} message={msg} />
      ))}

      {isLoading && (
        <div
          style={{
            display: "flex",
            alignItems: "center",
            gap: 8,
            padding: "4px 0",
          }}
        >
          <Spin size="small" />
          <Text type="secondary" italic>
            {t("page.aiChat.thinking")}
          </Text>
        </div>
      )}

      <div ref={bottomRef} />
    </div>
  );
}
