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
      streakTitle: "Daily check-in",
      streakSubtitle:
        "Check in every day to earn white coins and keep your energy streak alive",
      streakCurrentStreak: "Current streak",
      streakLongestStreak: "Longest streak",
      streakDays: "days",
      streakCheckIn: "Check in",
      streakCheckedInToday: "Checked in today",
      streakCheckInSuccess: "Check-in successful",
      streakReward: "Today's reward",
      streakSaverUsed: "Streak saver used this month",
      streakSaverAvailable: "Streak saver available this month",
      streakSaverResetIn: "Resets in {{days}} days (00:00 on the 1st, VN time)",
      streakDay1: "Day 1",
      streakDay2: "Day 2",
      streakDay3: "Day 3",
      streakDay4: "Day 4",
      streakDay5: "Day 5",
      streakDay6: "Day 6",
      streakDay7: "Day 7",
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
    aiTarot: {
      title: "AI Tarot",
      step2: {
        subtitle:
          "Clear your mind, focus on your question, then select {{count}} cards.",
      },
      cardCount: "Number of cards",
      cardCountOption: "{{count}} cards",
      positions: "Spread positions",
      questionType: "Question topic",
      questionTypes: {
        energy: "Energy",
        love: "Love",
        career: "Career",
        money: "Money",
      },
      cost: "Cost: {{cost}} white coins",
      balance: "Your balance: {{balance}} white coins",
      insufficientCoins:
        "You need at least {{cost}} white coins to continue. Check in daily to earn more.",
      continue: "Continue",
      back: "Back",
      saving: "Creating your AI reading…",
      position: {
        coreEnergy: "Core energy",
        challenges: "Challenges / obstacles",
        outcome: "Outcome / advice",
        yourStrength: "Your strength",
        future: "Future",
        hiddenInfluences: "Hidden influences",
        wayToFace: "The way to face it",
        focus: "What to focus on",
        past: "Past",
        nearFuture: "Near future",
        approach: "Suggested approach",
        needToKnow: "What you need to know",
        hopesFears: "Hopes / fears",
      },
      result: {
        title: "Reading result",
        overview: "Overview",
        advice: "Overall advice",
        noAnswer: "The reading content is being updated.",
        drawAgain: "Draw again",
      },
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
      parentTitle: "Your reading history",
      title: "1 card history",
      subtitle: "Review your past reflections and cosmic insights",
      empty: "No readings found yet",
      deleteDescription:
        "This reading will be permanently deleted. Are you sure you want to delete it?",
      deleteTitle: "Delete this reading?",
      deleteConfirm: "Delete reading",
      deleteSuccess: "Reading deleted",
    },
    historyAiTarot: {
      title: "AI Tarot history",
      subtitle: "Review your past AI readings and cosmic insights",
      empty: "No AI readings found yet",
      deleteTitle: "Delete this AI reading?",
      deleteDescription:
        "This AI reading will be permanently deleted. Are you sure you want to delete it?",
      deleteConfirm: "Delete reading",
      deleteSuccess: "AI reading deleted",
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
    wallet: {
      title: "My Wallet",
      subtitle:
        "Track your white and red coins. White coins in each batch expire after a period, so watch the deadlines.",
      batchesTitle: "White coin batches",
      batchesSubtitle:
        "Coins remaining in a batch are forfeited once it expires. Listed by expiry date, soonest first.",
      empty: "You don't have any white coin batches yet",
      colAmount: "Original amount",
      colRemaining: "Remaining",
      colExpiresAt: "Expires at",
      daysLeft: "{{days}} days left",
      expiresToday: "Expires today",
      convertTitle: "Convert red coins to white coins",
      convertSubtitle:
        "1 red coin = 2 white coins. The white coins you receive are added as a new batch.",
      convertRedCoins: "Red coins to convert",
      convertYouReceive: "You will receive",
      convertButton: "Convert",
      convertSuccess:
        "Converted {{redCoins}} red coins into {{whiteCoins}} white coins",
      convertExceedsBalance: "You only have {{balance}} red coins",
    },
  },
} as const;
