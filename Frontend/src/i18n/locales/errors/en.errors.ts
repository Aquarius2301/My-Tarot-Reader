export const enErrors = {
  error: {
    server: {
      internalServerError: "Internal server error. Please try again later.",
      badRequest: "Invalid request. Please check and try again.",
      unauthorized: "You are not authorized. Please log in to continue.",
      forbidden: "You do not have permission to access this resource.",
      notFound: "The requested resource was not found.",
      conflict: "Conflict with the current state. Please try again later.",
    },
    tarot: {
      drawnAlready:
        "You have already drawn a card today. Please come back at 0:00 local time.",
    },
  },
};
