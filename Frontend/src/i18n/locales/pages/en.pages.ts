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
} as const;
