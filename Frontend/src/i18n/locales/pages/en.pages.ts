export const enPages = {
  page: {
    home: {
      title: "Home page",
      heroTitle: "Listen to the Messages From",
      heroTitleHighlight: "the Universe & the Cards",
      heroDescription:
        "Decode your destiny, love, and career with AI-powered Tarot analysis. Get accurate answers and soul-healing advice instantly.",
      heroDrawCard: "Draw a Card Today",
      heroUpgradePro: "Upgrade to PRO",
      heroLoginNow: "Login Now",
    },
    tarot: {
      parentTitle: "Draw Tarot cards",
      title: "Draw 1 card",
      intro:
        "Clear your mind, focus on your question, then choose a single card.",
      yourCard: "Your card",
      upright: "Upright",
      reversed: "Reversed",
      drawAgain: "Draw again",
      saving: "Saving your card…",
      cooldown:
        "<strong>Your next draw will be available in {{hours}} hours {{minutes}} minutes. <btn>Log in now</btn> to draw more cards.</strong>",
    },
    library: {
      title: "Tarot card library",
      subtitle: "All 78 cards and their upright & reversed meanings",
      tabMajor: "Major Arcana",
      tabMinor: "Minor Arcana",
      tabWands: "Wands",
      tabCups: "Cups",
      tabSwords: "Swords",
      tabPentacles: "Pentacles",
      meaning: "Meanings of {{card}} · {{orientation}}",
    },
    history: {
      title: "Your reading history",
      subtitle: "Review your past reflections and cosmic insights",
      empty: "No readings found yet",
      deleteDescription:
        "This reading will be permanently deleted. Are you sure you want to delete it?",
      deleteTitle: "Delete this reading?",
      deleteConfirm: "Delete reading",
      deleteSuccess: "Reading deleted",
    },
    login: {
      title: "Sign in",
      heading: "Unlock your Tarot experience",
      subtitle:
        "Sign in to preserve your messages and connect more deeply with the energy of the universe.",
      welcomeTitle: "Welcome back",
      welcomeSubtitle: "Sign in quickly with your Google account.",
      googleSignIn: "Sign in with Google",
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
  },
} as const;
