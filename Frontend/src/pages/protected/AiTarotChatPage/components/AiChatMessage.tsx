import { Typography } from "antd";
import { AiMarkdown } from "@/components";
import type { AiChatMessage as AiChatMessageType } from "@/types";

const { Text } = Typography;

interface AiChatMessageProps {
  message: AiChatMessageType;
}

/**
 * A single chat message bubble. User messages are right-aligned with a
 * primary-colored background; AI messages are left-aligned with a neutral
 * background and render markdown content.
 */
export default function AiChatMessage({ message }: AiChatMessageProps) {
  const isUser = message.role === "user";

  return (
    <div
      style={{
        display: "flex",
        justifyContent: isUser ? "flex-end" : "flex-start",
        marginBottom: 12,
      }}
    >
      <div
        style={{
          maxWidth: "80%",
          padding: "10px 16px",
          borderRadius: 12,
          backgroundColor: isUser ? "var(--ai-chat-user-bg, #1668dc)" : "var(--ai-chat-ai-bg, #1f1f1f)",
          color: isUser ? "#fff" : "var(--ai-chat-ai-text, #e0e0e0)",
        }}
      >
        {isUser ? (
          <Text style={{ color: "inherit", whiteSpace: "pre-wrap" }}>
            {message.text}
          </Text>
        ) : (
          <AiMarkdown content={message.text} compact />
        )}
      </div>
    </div>
  );
}
