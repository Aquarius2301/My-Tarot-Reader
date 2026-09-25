import { useState } from "react";
import { App, Button, Card, InputNumber, Space, Typography, theme } from "antd";
import { SwapOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { CoinIcon } from "@/components";
import { RED_TO_WHITE_RATE } from "@/constants";
import { useConvertRedToWhite } from "@/hooks/api";
import { getErrorMessage } from "@/utils";

const { Title, Text } = Typography;

export interface ConvertCardProps {
  redCoin: number;
}

/**
 * Converts a chosen number of red coins into white coins (1 red = 2 white).
 * The granted white coins arrive as a new dated batch and are handled by the
 * backend; success refreshes the wallet and header balance caches.
 */
export default function ConvertCard({ redCoin }: ConvertCardProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const { message } = App.useApp();
  const { mutate, isPending } = useConvertRedToWhite();
  const [redCoins, setRedCoins] = useState<number | null>(null);

  const whiteCoins = redCoins === null ? null : redCoins * RED_TO_WHITE_RATE;
  const exceedsBalance = redCoins !== null && redCoins > redCoin;

  const handleConvert = () => {
    if (redCoins === null || redCoins <= 0) {
      return;
    }
    if (redCoins > redCoin) {
      message.error(t("page.wallet.convertExceedsBalance", { balance: redCoin }));
      return;
    }

    mutate(
      { redCoins },
      {
        onSuccess: () => {
          message.success(
            t("page.wallet.convertSuccess", {
              redCoins,
              whiteCoins: redCoins * RED_TO_WHITE_RATE,
            }),
          );
          setRedCoins(null);
        },
        onError: (error) => message.error(getErrorMessage(error)),
      },
    );
  };

  return (
    <Card
      style={{
        borderRadius: token.borderRadiusLG,
        boxShadow: token.boxShadowSecondary,
      }}
    >
      <Title level={4} style={{ margin: 0, marginBottom: 4 }}>
        <SwapOutlined style={{ color: token.colorPrimary, marginRight: 8 }} />
        {t("page.wallet.convertTitle")}
      </Title>
      <Text type="secondary">{t("page.wallet.convertSubtitle")}</Text>

      <div
        style={{
          display: "flex",
          flexWrap: "wrap",
          alignItems: "center",
          gap: token.marginMD,
          marginTop: token.marginMD,
        }}
      >
        <div style={{ flex: 1, minWidth: 220 }}>
          <Text style={{ display: "block", marginBottom: 4 }}>
            {t("page.wallet.convertRedCoins")}
          </Text>
          <Space.Compact style={{ width: "100%" }}>
            <InputNumber<number>
              min={1}
              max={redCoin}
              value={redCoins}
              onChange={setRedCoins}
              placeholder="0"
              status={exceedsBalance ? "error" : undefined}
              style={{ width: "100%" }}
              aria-label={t("page.wallet.convertRedCoins")}
            />
            <Button
              type="primary"
              loading={isPending}
              disabled={redCoins === null || redCoins <= 0}
              onClick={handleConvert}
            >
              {t("page.wallet.convertButton")}
            </Button>
          </Space.Compact>
          {exceedsBalance && (
            <Text
              type="danger"
              style={{ display: "block", marginTop: 4, fontSize: 12 }}
            >
              {t("page.wallet.convertExceedsBalance", { balance: redCoin })}
            </Text>
          )}
        </div>

        <div style={{ textAlign: "center", minWidth: 180 }}>
          <Text type="secondary" style={{ fontSize: 12, display: "block" }}>
            {t("page.wallet.convertYouReceive")}
          </Text>
          <Space size={8} style={{ marginTop: 4 }}>
            <CoinIcon variant="white" size={20} />
            <Text strong style={{ fontSize: 24 }}>
              {whiteCoins ?? 0}
            </Text>
          </Space>
        </div>
      </div>
    </Card>
  );
}