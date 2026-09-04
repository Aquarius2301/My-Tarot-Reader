import { Button, Card, Tag, Typography } from "antd";
import { UpOutlined, DownOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import type { SpreadRecommendation } from "@/types";

const { Text } = Typography;

interface AiChatSpreadProposalProps {
  /** The spread recommendation from the AI. */
  spread: SpreadRecommendation;
  /** Whether the card body is collapsed. */
  collapsed?: boolean;
  /** Toggle collapse state. */
  onToggleCollapse?: () => void;
  /** Callback when the user clicks "Draw Cards". */
  onDrawCards: () => void;
}

/**
 * Displays a spread recommendation with its positions and a call-to-action
 * button to start drawing cards. Supports collapsing the card body.
 */
export default function AiChatSpreadProposal({
  spread,
  collapsed = false,
  onToggleCollapse,
  onDrawCards,
}: AiChatSpreadProposalProps) {
  const { t } = useTranslation();

  return (
    <Card
      size="small"
      style={{
        margin: "12px 0",
        borderColor: "var(--ai-chat-spread-border, #4a3f6b)",
      }}
      title={
        <span
          style={{ cursor: onToggleCollapse ? "pointer" : "default", display: "flex", alignItems: "center", gap: 8 }}
          onClick={onToggleCollapse}
        >
          {t("page.aiChat.spreadProposalTitle")} —{" "}
          <Text strong style={{ fontSize: 15 }}>
            {spread.spreadName}
          </Text>
          {onToggleCollapse && (
            collapsed ? <DownOutlined style={{ fontSize: 12 }} /> : <UpOutlined style={{ fontSize: 12 }} />
          )}
        </span>
      }
    >
      {!collapsed && (
        <>
          <Text type="secondary" style={{ display: "block", marginBottom: 12 }}>
            {t("page.aiChat.spreadProposalDescription")}
          </Text>

          <div style={{ display: "flex", flexWrap: "wrap", gap: 8, marginBottom: 16 }}>
            {spread.positions.map((pos) => (
              <Tag key={pos.position} color="purple">
                {pos.position}. {pos.name}
              </Tag>
            ))}
          </div>

          <Text type="secondary" style={{ display: "block", marginBottom: 12 }}>
            {t("page.aiChat.drawCardsHint", { count: spread.cardCount })}
          </Text>

          <Button type="primary" onClick={onDrawCards}>
            {t("page.aiChat.drawCardsButton")}
          </Button>
        </>
      )}
    </Card>
  );
}
