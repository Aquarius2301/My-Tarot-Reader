export const enErrors = {
  error: {
    system: {
      internalServerError: "Internal server error. Please try again later.",
      badRequest: "Invalid request. Please check and try again.",
      unauthorized: "You are not authorized. Please log in to continue.",
      forbidden: "You do not have permission to access this resource.",
      notFound: "The requested resource was not found.",
      conflict: "Conflict with the current state. Please try again later.",
    },
    tarot: {
      drawnAlready: "You have already drawn a card for this reading.",
      notFound: "The reading was not found.",
    },
    streak: {
      alreadyCheckedIn: "You have already checked in today.",
    },
    aiTarot: {
      invalidCardCount: "Invalid number of cards.",
      invalidCard: "One or more cards are invalid.",
      invalidLocale: "Unsupported language.",
      invalidQuestionType: "Unsupported question topic.",
      readingNotFound: "This AI reading was not found.",
      readingCountNotFound: "This AI tarot spread was not found.",
      readingTypeNotFound: "This AI tarot topic was not found.",
      cardCountExists: "This card count already exists.",
      readingTypeCodeExists: "The topic code already exists.",
      invalidReadingCount: "Invalid spread configuration.",
      invalidReadingType: "Invalid topic configuration.",
      generationFailed: "Could not create the AI reading. Please try again.",
    },
    aiDeepTarot: {
      invalidCardCount: "Invalid number of cards.",
      invalidCard: "One or more cards are invalid.",
      invalidLocale: "Unsupported language.",
      invalidQuestion: "Your question is not valid.",
      invalidOption: "Your options are not valid.",
      invalidTimeFrame: "Unsupported timeframe.",
      unsafeContent:
        "A tarot reading cannot cover this topic. If you are struggling with thoughts of harming yourself, please talk to someone you trust or contact a crisis hotline in your country.",
      questionNotSupported:
        "Your question is not a decision we can read. Please describe the real choice you are facing in your own words.",
      readingNotFound: "This deep tarot reading was not found.",
      generationFailed: "Could not create the deep tarot reading. Try again.",
    },
    wallet: {
      insufficientCoins: "You don't have enough white coins for this action.",
      insufficientRedCoin: "You don't have enough red coins for this action.",
      invalidAmount: "The number of coins is invalid.",
      walletNotFound: "Your wallet was not found.",
    },
    shop: {
      invalidPackage: "This package is not available.",
      packageNotFound: "The package was not found.",
      packageCodeExists: "The package code already exists.",
      orderNotFound: "The order was not found.",
      createPaymentFailed:
        "Could not create the payment. Please try again.",
      invalidWebhookSignature: "Invalid payment notification signature.",
      confirmWebhookFailed:
        "Could not confirm the payment. Please try again.",
      payOsError: "The payment provider reported an error. Please try again.",
    },
  },
} as const;
