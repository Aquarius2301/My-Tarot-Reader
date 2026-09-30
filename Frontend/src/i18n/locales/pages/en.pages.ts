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
    aiDeepTarot: {
      parentTitle: "Deep tarot",
      twelveHouses: {
        title: "12 houses spread",
        subtitle:
          "12 cards settle into the twelve houses one by one, reflecting the energy and the current state of every area of your life.",
        what: {
          title: "What are the 12 Houses in Tarot?",
          body: "In astrology, a person's life is divided into 12 pieces representing 12 areas: from the self, money and love to family, career and spirituality. With the 12 Houses spread, the tarot cards take their seat in each house in turn to reflect the energy and the current state of that area of your life.",
        },
        why: {
          title: "What is this spread for?",
          items: {
            overview:
              "A complete point of view: instead of answering a single isolated question (like \"Do they like me?\"), this spread shows you the big picture of every side of life.",
            blockage:
              "Find the bottleneck: quickly spot which area is growing well and which one is stuck and needs your attention.",
            forecast:
              "Forecast the energy: help you prepare mentally and choose the right direction for what is coming.",
          },
        },
        when: {
          title: "When should you look at the 12 Houses?",
          items: {
            milestone:
              "At a new milestone: the start of a new year, your birthday, or the beginning of a new cycle in life.",
            lost:
              "When you feel directionless: everything feels off but you cannot pin down what is actually wrong (work, love, or your mental health).",
            selfReview:
              "When you want to reassess yourself: for the moments when you want to pause, look back at your whole life and plan a more balanced way forward.",
          },
        },
      },
      houses: {
        title: "The twelve houses in your spread",
        hint: "Every card you pick will be interpreted in the light of its house.",
      },
      house: {
        "house-1": "House 1 · Self and identity",
        "house-2": "House 2 · Money and personal values",
        "house-3": "House 3 · Mind and communication",
        "house-4": "House 4 · Home and family",
        "house-5": "House 5 · Creativity and joy",
        "house-6": "House 6 · Health and daily routine",
        "house-7": "House 7 · Partnership and marriage",
        "house-8": "House 8 · Transformation and intimacy",
        "house-9": "House 9 · Belief and expansion",
        "house-10": "House 10 · Career and reputation",
        "house-11": "House 11 · Community and shared goals",
        "house-12": "House 12 · Subconscious and retreat",
      },
      draw: {
        subtitle:
          "Clear your mind, focus on your question, then pick all 12 cards — each one fills a house in the order you draw it.",
      },
      cardCount: "{{count}} cards · {{houses}} houses",
      cost: "Cost: {{cost}} red coins",
      balance: "Your balance: {{balance}} red coins",
      insufficientCoins:
        "You need at least {{cost}} red coins to continue. Check in daily to earn more.",
      continue: "Continue",
      back: "Back",
      saving: "Creating your deep tarot reading…",
      result: {
        title: "Deep tarot reading result",
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
    historyAiDeepTarot: {
      title: "Deep tarot history",
      subtitle: "Review your past deep tarot spreads",
      empty: "No deep tarot readings found yet",
      deleteTitle: "Delete this reading?",
      deleteDescription:
        "This deep tarot reading will be permanently deleted. Are you sure you want to delete it?",
      deleteConfirm: "Delete reading",
      deleteSuccess: "Deep tarot reading deleted",
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
