import { CheckOutlined, CopyOutlined } from "@ant-design/icons";
import { copyTextToClipboard } from "@/utils";
import { App, Button, type ButtonProps } from "antd";
import { useState } from "react";
import { useTranslation } from "react-i18next";

export interface CopyButtonProps extends ButtonProps {
  text: string;
}

/** Copies the given text to the clipboard, showing a success message when done. */
export default function CopyButton({ text, ...rest }: CopyButtonProps) {
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
    <Button icon={copied ? <CheckOutlined /> : <CopyOutlined />} onClick={handleCopy} {...rest}>
      {copied ? t("component.copyButton.copied") : t("component.copyButton.copy")}
    </Button>
  );
}