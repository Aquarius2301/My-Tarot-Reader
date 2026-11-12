import { useState } from "react";
import { Tabs, theme } from "antd";
import { useTranslation } from "react-i18next";
import {
  MAJOR_ARCANA,
  MINOR_ARCANA,
  MINOR_SUITS,
  type TarotCardCode,
} from "@/constants";
import { AppTitle } from "@/components";
import { CardLibraryGrid } from "./CardLibraryGrid";
import { MeaningModal } from "./MeaningModal";

/** Returns the codes of a given minor-arcana suit (e.g. "min-wands-1"). */
function codesOfSuit(suit: string): readonly TarotCardCode[] {
  return MINOR_ARCANA.filter((code) => code.startsWith(`min-${suit}-`));
}

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
      <AppTitle
        title={t("page.library.title")}
        subtitle={t("page.library.subtitle")}
      />

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

      <MeaningModal
        selectedCard={selectedCard}
        onSelectedCard={setSelectedCard}
      />
    </div>
  );
}
