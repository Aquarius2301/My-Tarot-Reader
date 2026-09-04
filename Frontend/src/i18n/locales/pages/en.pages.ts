export const enPages = {
  page: {
    home: {
      title: "Home Page",
      heroTitle: "Listen to the Messages From",
      heroTitleHighlight: "the Universe & the Cards",
      heroDescription:
        "Decode your destiny, love, and career with AI-powered Tarot analysis. Get accurate answers and soul-healing advice instantly.",
      heroDrawCard: "Draw a Card Today",
      heroUpgradePro: "Upgrade to PRO",
      heroLoginNow: "Login Now",
    },
    login: {
      title: "Sign in",
      heading: "Unlock your Tarot experience",
      subtitle:
        "Sign in to preserve your messages and connect more deeply with the energy of the universe.",
      welcomeTitle: "Welcome back",
      welcomeSubtitle: "Sign in quickly with your Google account.",
      googleLoginError: "Google sign-in failed. Please try again.",
      benefit: {
        ai: {
          title: "In-depth readings with AI",
          description:
            "Get personalized interpretations tailored to your questions and psychological context.",
        },
        history: {
          title: "Save your reading history",
          description:
            "Review all the cards you've drawn and your energy journey over time.",
        },
        daily: {
          title: "Daily energy insights",
          description:
            "Receive a daily Tarot message and emotional-balance suggestions.",
        },
      },
    },
    draw: {
      title: "Draw Tarot Cards",
      oneCard: { title: "Draw 1 Card" },
    },
    history: {
      title: "Your Reading History",
      subtitle: "Review your past reflections and cosmic insights",
      empty: "No readings found yet",
      reversed: "Reversed",
      upright: "Upright",
      deleteAria: "Delete {{card}} reading",
      deleteConfirmTitle: "Delete this reading?",
      deleteOk: "Delete",
      deleteCancel: "Cancel",
      deleteSuccess: "Reading deleted",
    },
    aiDraw: {
      title: "AI Tarot Reading",
      intro:
        "Choose the number of cards, question type and language, then draw to receive a deep AI interpretation.",
      cardPositionsLabel: "Card positions",
      cardPositions: {
        three: "Core energy, Challenges / obstacles, Outcome / advice",
        five: "Core energy, Challenges / obstacles, Your strength, Future, Outcome / advice",
        seven:
          "Core energy, Challenges / obstacles, Your strength, Hidden influences, The way to face it, Future, Outcome / advice",
        ten: "Core energy, Challenges / obstacles, What to focus on, Past, Your strength, Near future, Suggested approach, What you need to know, Hopes / fears, Outcome / advice",
      },

      languageLabel: "Interpretation language",
      language: {
        vi: "Tiếng Việt",
        en: "English",
      },
      confirmButton: "Get Reading",
      saving: "Interpreting...",
      resultTitle: "Reading Result",
      yourCards: "Your Cards",
      drawAgain: "Draw Again",
      invalidSelection: "Please select all the required cards before reading.",
    },
    aiChat: {
      title: "AI Tarot Chat",
      subtitle:
        "Ask anything and let AI guide your tarot reading",
      inputPlaceholder: "Type your tarot question...",
      send: "Send",
      thinking: "AI is thinking...",
      spreadProposalTitle: "Spread Recommended",
      spreadProposalDescription:
        "Based on your question, I recommend the following spread:",
      drawCardsButton: "Draw Cards",
      drawCardsHint: "Select {{count}} cards from the deck below",
      readingTitle: "Your Reading",
      readingYourCards: "Your Cards",
      newChat: "Start New Chat",
    },
  },
  nav: {
    login: "Sign in",
    logout: "Sign out",
    theme: "Theme",
    language: "Language",
    darkMode: "Dark mode",
    lightMode: "Light mode",
    guest: "Guest",
    vietnamese: "Vietnamese",
    english: "English",
  },
  select: {
    questionTypeLabel: "Question type",
    questionType: {
      energy: "Energy",
      love: "Love",
      career: "Career",
      money: "Money",
    },
    cardCountLabel: "Number of cards",
    cardCount: {
      three: "3 cards",
      five: "5 cards",
      seven: "7 cards",
      ten: "10 cards",
    },
  },
} as const;
