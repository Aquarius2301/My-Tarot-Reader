import { type KeyboardEvent } from "react";
import { Card, Flex, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import { TarotCard } from "@/components";
import type { TarotCardCode } from "@/constants";

const { Text } = Typography;

interface CardLibraryGridProps {
  codes: readonly TarotCardCode[];
  onSelectCard: (code: TarotCardCode) => void;
}

/**
 * Responsive auto-fill grid of tarot cards. Each cell is a single
 * keyboard-focusable element (Enter/Space opens the meanings dialog); the
 * inner TarotCard is presentational so focus never gets trapped on a nested
 * interactive element.
 */
export function CardLibraryGrid({ codes, onSelectCard }: CardLibraryGridProps) {
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
          <Card
            hoverable
            key={code}
            role="button"
            tabIndex={0}
            aria-label={t("page.library.viewCardAria", { card: name })}
            onClick={() => onSelectCard(code)}
            onKeyDown={handleKeyDown(code)}
            style={{
              cursor: "pointer",
            }}
          >
            <Flex vertical align="center">
              <div
                style={{
                  marginBottom: token.marginMD,
                }}
              >
                <TarotCard cardCode={code} isUpright size="sm" />
              </div>
              <Text strong style={{ textAlign: "center" }}>
                {name}
              </Text>
            </Flex>
          </Card>
        );
      })}
    </div>
  );
}
