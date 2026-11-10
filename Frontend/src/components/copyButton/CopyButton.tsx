import { App, Button } from "antd";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { CopyButtonProps } from "./types";
import { CheckOutlined, CopyOutlined } from "@ant-design/icons";
import { copyTextToClipboard } from "@/utils";

/** Copies the given text to the clipboard, showing a success message when done. */
export function CopyButton({ text, ...rest }: CopyButtonProps) {
  const { t } = useTranslation();
  const { message } = App.useApp();
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    const ok = await copyTextToClipboard(text);
    if (ok) {
      setCopied(true);
      message.success(t("component.copyButton.copied"));
      setTimeout(() => setCopied(false), 1500);
    } else {
      message.error(t("component.copyButton.failed"));
    }
  };

  return (
    <Button
      icon={copied ? <CheckOutlined /> : <CopyOutlined />}
      onClick={handleCopy}
      style={{ padding: 16 }}
      {...rest}
    >
      {copied
        ? t("component.copyButton.copied")
        : t("component.copyButton.copy")}
    </Button>
  );
}
