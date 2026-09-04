import { Typography } from "antd";
import ReactMarkdown from "react-markdown";
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
          <ReactMarkdown
            components={{
              h1: ({ children }) => (
                <Typography.Title level={4} style={{ color: "inherit", marginBottom: 8 }}>
                  {children}
                </Typography.Title>
              ),
              h2: ({ children }) => (
                <Typography.Title level={5} style={{ color: "inherit", marginBottom: 6 }}>
                  {children}
                </Typography.Title>
              ),
              h3: ({ children }) => (
                <Typography.Title level={5} style={{ color: "inherit", marginBottom: 4 }}>
                  {children}
                </Typography.Title>
              ),
              p: ({ children }) => (
                <Typography.Paragraph
                  style={{ color: "inherit", marginBottom: 8, fontSize: 15, lineHeight: 1.7 }}
                >
                  {children}
                </Typography.Paragraph>
              ),
              strong: ({ children }) => (
                <Text strong style={{ color: "inherit" }}>
                  {children}
                </Text>
              ),
              li: ({ children }) => (
                <li style={{ marginBottom: 4, color: "inherit" }}>{children}</li>
              ),
            }}
          >
            {message.text}
          </ReactMarkdown>
        )}
      </div>
    </div>
  );
}
