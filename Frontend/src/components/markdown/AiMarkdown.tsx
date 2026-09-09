import ReactMarkdown from "react-markdown";
import { Typography } from "antd";

const { Title, Text, Paragraph } = Typography;

export interface AiMarkdownProps {
  /** The AI-generated markdown content to render. */
  content: string;
  /** Compact rendering for tight spaces (e.g. chat bubbles): smaller
   * heading levels/type and inherited colors instead of the default ones. */
  compact?: boolean;
}

/**
 * Renders AI-generated markdown with a consistent set of typography
 * mappings shared across the app (AI reading results, history modals,
 * chat bubbles). Style is preserved so all AI text looks identical.
 * `compact` targets low-contrast/constrained containers such as chat bubbles.
 */
export default function AiMarkdown({ content, compact = false }: AiMarkdownProps) {
  const color = compact ? { color: "inherit" as const } : undefined;

  return (
    <ReactMarkdown
      components={{
        h1: ({ children }) => (
          <Title level={compact ? 4 : 3} style={{ ...color, marginBottom: compact ? 8 : undefined }}>
            {children}
          </Title>
        ),
        h2: ({ children }) => (
          <Title level={compact ? 5 : 4} style={{ ...color, marginBottom: compact ? 6 : undefined }}>
            {children}
          </Title>
        ),
        h3: ({ children }) => (
          <Title level={5} style={{ ...color, marginBottom: compact ? 4 : undefined }}>
            {children}
          </Title>
        ),
        h4: ({ children }) => (
          <Title level={5} style={color}>
            {children}
          </Title>
        ),
        p: ({ children }) => (
          <Paragraph
            style={{
              marginBottom: compact ? 8 : 12,
              fontSize: compact ? 15 : 16,
              lineHeight: 1.7,
              ...color,
            }}
          >
            {children}
          </Paragraph>
        ),
        strong: ({ children }) => (
          <Text strong style={color}>
            {children}
          </Text>
        ),
        li: ({ children }) => (
          <li style={{ marginBottom: 4, color: "inherit" }}>{children}</li>
        ),
      }}
    >
      {content}
    </ReactMarkdown>
  );
}
