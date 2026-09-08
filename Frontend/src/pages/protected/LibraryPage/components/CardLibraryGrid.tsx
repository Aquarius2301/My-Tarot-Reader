import { type KeyboardEvent } from "react";
import { Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import { TarotCard } from "@/components";
import type { TarotCardCode } from "@/constants";

const { Text } = Typography;

export interface CardLibraryGridProps {
  codes: readonly TarotCardCode[];
  onSelectCard: (code: TarotCardCode) => void;
}

/**
 * Responsive auto-fill grid of tarot cards. Each cell is a single
 * keyboard-focusable element (Enter/Space opens the meanings dialog); the
 * inner TarotCard is presentational so focus never gets trapped on a nested
 * interactive element.
 */
export default function CardLibraryGrid({
  codes,
  onSelectCard,
}: CardLibraryGridProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const handleKeyDown =
    (code: TarotCardCode) => (e: KeyboardEvent<HTMLDivElement>) => {
      if (e.key === "Enter" || e.key === " ") {
        e.preventDefault();
        onSelectCard(code);
      }
    };

  return (
    <div
      style={{
        display: "grid",
        gridTemplateColumns: "repeat(auto-fill, minmax(220px, 1fr))",
        gap: token.marginLG,
      }}
    >
      {codes.map((code) => {
        const name = t(`tarot.meaning.${code}.name`);

        return (
          <div
            key={code}
            role="button"
            tabIndex={0}
            aria-haspopup="dialog"
            aria-label={t("page.library.viewCardAria", { card: name })}
            onClick={() => onSelectCard(code)}
            onKeyDown={handleKeyDown(code)}
            style={{
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
              padding: `${token.paddingLG}px ${token.paddingMD}px ${token.paddingMD}px`,
              borderRadius: token.borderRadiusLG,
              border: `1px solid ${token.colorBorderSecondary}`,
              backgroundColor: token.colorBgContainer,
              backdropFilter: "blur(10px)",
              boxShadow: token.boxShadowTertiary,
              cursor: "pointer",
              transition:
                "all 0.3s cubic-bezier(0.4, 0, 0.2, 1)",
            }}
          >
            <div
              style={{
                marginBottom: token.marginMD,
                filter: `drop-shadow(0 4px 12px ${token.colorPrimary}26)`,
              }}
            >
              <TarotCard cardCode={code} isUpright size="sm" />
            </div>
            <Text strong style={{ textAlign: "center" }}>
              {name}
            </Text>
          </div>
        );
      })}
    </div>
  );
}