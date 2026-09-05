import { useState, useCallback } from "react";
import { App, Card, Grid, Spin, Typography } from "antd";
import { CommentOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { TarotDeck, type SpreadResultItem } from "@/components";
import { useCreateChatSession, useSendChatMessage, useSubmitChatReading } from "@/hooks/api";
import { useLanguageStore } from "@/hooks/store";
import { getErrorMessage } from "@/utils";
import type {
  AiChatMessage,
  SpreadRecommendation,
} from "@/types";
import {
  AiChatInput,
  AiChatMessageList,
  AiChatSpreadProposal,
  AiChatReadingResult,
} from "./components";

const { Title, Text } = Typography;
const { useBreakpoint } = Grid;

/** The current phase of the chat page. */
type PagePhase = "idle" | "chatting" | "drawing" | "reading";

export default function AiTarotChatPage() {
  const { t } = useTranslation();
  const { message: messageApi } = App.useApp();
  const screens = useBreakpoint();
  const isMobile = !screens.md;

  const language = useLanguageStore((s) => s.mode);

  // --- Mutations ---
  const createSession = useCreateChatSession();
  const sendMessage = useSendChatMessage();
  const submitReading = useSubmitChatReading();

  // --- State ---
  const [phase, setPhase] = useState<PagePhase>("idle");
  const [historyId, setHistoryId] = useState<string | null>(null);
  const [messages, setMessages] = useState<AiChatMessage[]>([]);
  const [question, setQuestion] = useState(""); // idle-phase draft
  const [draft, setDraft] = useState(""); // chatting-phase draft
  const [spreadRecommendation, setSpreadRecommendation] =
    useState<SpreadRecommendation | null>(null);
  const [showSpread, setShowSpread] = useState(true);
  const [selectedCards, setSelectedCards] = useState<SpreadResultItem[]>([]);
  const [readingAnswer, setReadingAnswer] = useState<string | null>(null);
  const [deckKey, setDeckKey] = useState(0);

  const isLoading = createSession.isPending || sendMessage.isPending || submitReading.isPending;

  // --- Handlers ---
  const handleSendQuestion = useCallback(
    (question: string) => {
      createSession.mutate(
        { question, language },
        {
          onSuccess: (data) => {
            setHistoryId(data.historyId);
            setMessages([
              { role: "user", text: question },
              { role: "model", text: data.answer },
            ]);
            setPhase("chatting");
            setQuestion(""); // idle input unmounts on phase change; hygiene
          },
          onError: (error) => {
            messageApi.error(getErrorMessage(error));
          },
        },
      );
    },
    [createSession, language, messageApi],
  );

  const handleSendFollowUp = useCallback(
    (text: string) => {
      if (!historyId) return;

      setSpreadRecommendation(null);

      const optimisticIndex = messages.length; // pre-append index
      setMessages((prev) => [...prev, { role: "user", text }]);

      sendMessage.mutate(
        { historyId, message: text, language },
        {
          onSuccess: (data) => {
            setMessages((prev) => [
              ...prev,
              { role: "model", text: data.answer },
            ]);

            if (data.spreadRecommendation) {
              setSpreadRecommendation(data.spreadRecommendation);
              setShowSpread(true);
            }
            setDraft(""); // clear only on success
          },
          onError: (error) => {
            // Roll back the optimistic user message so it isn't duplicated;
            // the typed text stays in the input (draft is never cleared).
            setMessages((prev) => prev.slice(0, optimisticIndex));
            messageApi.error(getErrorMessage(error));
          },
        },
      );
    },
    [historyId, sendMessage, language, messageApi, messages],
  );

  const handleConfirmDraw = useCallback(
    (cards: SpreadResultItem[]) => {
      if (!historyId) return;

      setSelectedCards(cards);

      submitReading.mutate(
        {
          historyId,
          cards: cards.map((c) => ({ code: c.cardCode, isReversed: c.isReversed })),
          language,
        },
        {
          onSuccess: (data) => {
            setReadingAnswer(data.answer);
            setPhase("reading");
          },
          onError: (error) => {
            messageApi.error(getErrorMessage(error));
          },
        },
      );
    },
    [historyId, submitReading, language, messageApi],
  );

  const handleStartDrawing = useCallback(() => {
    setPhase("drawing");
  }, []);

  const handleNewChat = useCallback(() => {
    setPhase("idle");
    setHistoryId(null);
    setMessages([]);
    setSpreadRecommendation(null);
    setSelectedCards([]);
    setReadingAnswer(null);
    setQuestion("");
    setDraft("");
    setDeckKey((k) => k + 1);
  }, []);

  // --- Render ---

  // Reading phase: show the final result.
  if (phase === "reading" && readingAnswer) {
    return (
      <AiChatReadingResult
        answer={readingAnswer}
        cards={selectedCards}
        onNewChat={handleNewChat}
      />
    );
  }

  // Idle phase: show the initial question input.
  if (phase === "idle") {
    return (
      <div
        style={{
          maxWidth: 640,
          margin: "0 auto",
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          paddingTop: isMobile ? 32 : 64,
        }}
      >
        <CommentOutlined
          style={{ fontSize: 48, color: "var(--ai-chat-icon-color, #7c5cbf)", marginBottom: 16 }}
        />
        <Title level={3} style={{ textAlign: "center" }}>
          {t("page.aiChat.title")}
        </Title>
        <Text
          type="secondary"
          style={{ display: "block", textAlign: "center", marginBottom: 24 }}
        >
          {t("page.aiChat.subtitle")}
        </Text>

        <Card style={{ width: "100%" }}>
          <AiChatInput
            value={question}
            onChange={setQuestion}
            onSend={handleSendQuestion}
            disabled={isLoading}
          />
        </Card>

        {createSession.isPending && <Spin fullscreen />}
      </div>
    );
  }

  // Chatting / Drawing phases: show chat interface + optional spread/draw UI.
  return (
    <div
      style={{
        maxWidth: 720,
        margin: "0 auto",
        display: "flex",
        flexDirection: "column",
        height: isMobile ? "calc(100vh - 120px)" : "calc(100vh - 160px)",
      }}
    >
      <Title level={4} style={{ marginBottom: 8, textAlign: "center" }}>
        {t("page.aiChat.title")}
      </Title>

      <AiChatMessageList messages={messages} isLoading={sendMessage.isPending} />

      {phase === "chatting" && spreadRecommendation && (
        <AiChatSpreadProposal
          spread={spreadRecommendation}
          collapsed={!showSpread}
          onToggleCollapse={() => setShowSpread((s) => !s)}
          onDrawCards={handleStartDrawing}
        />
      )}

      {phase === "drawing" && spreadRecommendation && (
        <div style={{ padding: "12px 0" }}>
          <Text strong style={{ display: "block", marginBottom: 8 }}>
            {t("page.aiChat.drawCardsHint", { count: spreadRecommendation.cardCount })}
          </Text>
          <TarotDeck
            key={deckKey}
            limit={spreadRecommendation.cardCount}
            cardSize={isMobile ? "sm" : "md"}
            onConfirm={handleConfirmDraw}
          />
        </div>
      )}

      {phase === "chatting" && (
        <div style={{ paddingTop: 12, borderTop: "1px solid var(--ai-chat-divider, #303030)" }}>
          <AiChatInput
            value={draft}
            onChange={setDraft}
            onSend={handleSendFollowUp}
            disabled={isLoading}
          />
        </div>
      )}

      {submitReading.isPending && <Spin fullscreen />}
    </div>
  );
}
