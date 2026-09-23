import { CheckOutlined, CopyOutlined } from "@ant-design/icons";
import { App, Button, Card, Typography } from "antd";
import { TAROT_SECTIONS, type TarotCardCode } from "@/constants";
import { useTranslation } from "react-i18next";
import { useState } from "react";
import i18n from "@/i18n";
import { copyTextToClipboard } from "@/utils";

interface TarotMeaningCardProps {
  cardCode: TarotCardCode;
  isReversed: boolean;
}

export default function TarotMeaningCard({
  cardCode,
  isReversed,
}: TarotMeaningCardProps) {
  const { t } = useTranslation();
  const { message } = App.useApp();
  const [copied, setCopied] = useState(false);

  const meaningKey = (section: string) =>
    `tarot.meaning.${cardCode}.${isReversed ? "reversed" : "upright"}.${section}`;

  const sectionText = (section: string) => {
    const key = meaningKey(section);
    return i18n.exists(key) ? t(key) : t(`tarot.placeholder.${section}`);
  };

  const copyText = () => {
    const name = t(`tarot.meaning.${cardCode}.name`);
    const orientation = isReversed
      ? t("tarot.position.reversed")
      : t("tarot.position.upright");

    return [
      `${name} · ${orientation}`,
      ...TAROT_SECTIONS.map(
        (section) => `${t(`tarot.section.${section}`)}\n${sectionText(section)}`,
      ),
    ].join("\n\n");
  };

  const handleCopy = async () => {
    const ok = await copyTextToClipboard(copyText());
    if (ok) {
      setCopied(true);
      message.success(t("component.copyButton.copied"));
      setTimeout(() => setCopied(false), 1500);
    } else {
      message.error(t("component.copyButton.failed"));
    }
  };

  return (
    <Card
      style={{ textAlign: "left", marginTop: 16 }}
      extra={
        <Button
          icon={copied ? <CheckOutlined /> : <CopyOutlined />}
          onClick={handleCopy}
        >
          {copied
            ? t("component.copyButton.copied")
            : t("component.copyButton.copy")}
        </Button>
      }
    >
      {TAROT_SECTIONS.map((section) => {
        const text = sectionText(section);
        return (
          <div key={section} style={{ marginBottom: 16 }}>
            <Typography.Text strong>
              {t(`tarot.section.${section}`)}
            </Typography.Text>
            <Typography.Paragraph style={{ marginTop: 4 }}>
              {text}
            </Typography.Paragraph>
          </div>
        );
      })}
    </Card>
  );
}