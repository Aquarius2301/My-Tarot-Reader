import { Button, Input } from "antd";
import { SendOutlined } from "@ant-design/icons";
import { useCallback } from "react";
import { useTranslation } from "react-i18next";

const { TextArea } = Input;

interface AiChatInputProps {
  /** Current input text (controlled). */
  value: string;
  /** Called whenever the user edits the text. */
  onChange: (value: string) => void;
  /** Callback when the user submits a message. */
  onSend: (message: string) => void;
  /** Whether the input is disabled (e.g. while AI is thinking). */
  disabled?: boolean;
  /** Placeholder text for the input. */
  placeholder?: string;
}

/**
 * Chat input with a text area and send button.
 * Enter sends the message; Shift+Enter inserts a newline.
 */
export default function AiChatInput({
  value,
  onChange,
  onSend,
  disabled = false,
  placeholder,
}: AiChatInputProps) {
  const { t } = useTranslation();

  const handleSend = useCallback(() => {
    const trimmed = value.trim();
    if (!trimmed || disabled) return;
    onSend(trimmed);
  }, [value, disabled, onSend]);

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
      if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault();
        handleSend();
      }
    },
    [handleSend],
  );

  return (
    <div style={{ display: "flex", gap: 8, alignItems: "flex-end" }}>
      <TextArea
        value={value}
        onChange={(e) => onChange(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder={placeholder ?? t("page.aiChat.inputPlaceholder")}
        autoSize={{ minRows: 2, maxRows: 6 }}
        disabled={disabled}
        style={{ flex: 1, resize: "vertical" }}
      />
      <Button
        type="primary"
        icon={<SendOutlined />}
        onClick={handleSend}
        disabled={disabled || !value.trim()}
        style={{ height: 40 }}
      />
    </div>
  );
}
