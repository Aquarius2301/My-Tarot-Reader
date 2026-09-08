import { useState } from "react";
import { Tabs, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import {
  MAJOR_ARCANA,
  MINOR_ARCANA,
  MINOR_SUITS,
  type TarotCardCode,
} from "@/constants";
import CardLibraryGrid from "./components/CardLibraryGrid";
import TarotCardMeaningTabsModal from "./components/TarotCardMeaningTabsModal";

const { Title, Text } = Typography;

/** Returns the codes of a given minor-arcana suit (e.g. "min-wands-1"). */
function codesOfSuit(suit: string): readonly TarotCardCode[] {
  return MINOR_ARCANA.filter((code) => code.startsWith(`min-${suit}-`));
}

/**
 * Tarot card library: all 78 cards grouped into Major Arcana and each Minor
 * Arcana suit. Clicking a card opens its upright/reversed meanings modal.
 */
export default function LibraryPage() {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const [selectedCard, setSelectedCard] = useState<TarotCardCode | null>(null);

  const suitTabs = MINOR_SUITS.map((suit) => ({
    key: suit,
    label: t(`page.library.tab${suit[0].toUpperCase()}${suit.slice(1)}`),
    children: (
      <CardLibraryGrid
        codes={codesOfSuit(suit)}
        onSelectCard={setSelectedCard}
      />
    ),
  }));

  return (
    <div
      style={{
        maxWidth: 1080,
        margin: "0 auto",
        padding: `${token.paddingLG}px ${token.paddingMD}px`,
      }}
    >
      {/* Header Section */}
      <div style={{ textAlign: "center", marginBottom: token.marginXL }}>
        <Title level={2} style={{ margin: 0 }}>
          {t("page.library.title")}
        </Title>
        <Text type="secondary">{t("page.library.subtitle")}</Text>
      </div>

      {/* Two top-level tabs: Major Arcana + Minor Arcana (whose 4 suits are
      nested tabs underneath). */}
      <Tabs
        centered
        items={[
          {
            key: "major",
            label: t("page.library.tabMajor"),
            children: (
              <CardLibraryGrid
                codes={MAJOR_ARCANA}
                onSelectCard={setSelectedCard}
              />
            ),
          },
          {
            key: "minor",
            label: t("page.library.tabMinor"),
            children: <Tabs centered items={suitTabs} />,
          },
        ]}
      />

      {/* Card meaning modal with upright / reversed tabs. The key remounts the
      modal per card so its upright/reversed orientation state never carries
      over to a newly opened card. */}
      <TarotCardMeaningTabsModal
        key={selectedCard ?? "closed"}
        open={!!selectedCard}
        cardCode={selectedCard ?? undefined}
        onClose={() => setSelectedCard(null)}
      />
    </div>
  );
}