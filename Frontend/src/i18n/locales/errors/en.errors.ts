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
      readingNotFound: "This AI reading was not found.",
      generationFailed: "Could not create the AI reading. Please try again.",
    },
    wallet: {
      insufficientCoins: "You don't have enough white coins for this action.",
    },
  },
} as const;
