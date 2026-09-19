export const enComponents = {
  component: {
    mainLayout: {
      login: "Sign in",
      logout: "Sign out",
      theme: "Theme",
      language: "Language",
      darkMode: "Dark mode",
      lightMode: "Light mode",
      guest: "Guest",
      vietnamese: "Vietnamese",
      english: "English",
      whiteCoin: "White coins",
      redCoin: "Red coins",
    },
    boot: {
      loading: "Loading...",
    },
    deck: {
      instruction: "Hover to preview, click to select a card",
      reshuffle: "Reshuffle",
      confirm: "Confirm",
      selected: "Selected {{count}}/{{limit}}",
      limitReached: "You can select at most {{n}} cards",
      noEnoughCoin: "You don't have enough coins to draw this spread",
    },
    modal: {
      ok: "OK",
      cancel: "Cancel",
      delete: "Delete",
    },
    error: {
      offlineTitle: "You're offline",
      offlineDesc: "Please check your internet connection and try again.",
      serverTitle: "Server error",
      serverDesc:
        "Something went wrong on our end. Please try again in a moment.",
      timeoutTitle: "Connection timed out",
      timeoutDesc:
        "The server took too long to respond. Please check your connection and try again.",
      genericTitle: "Something went wrong",
      genericDesc: "An unexpected error occurred. Please try again.",
      retry: "Try again",
    },
  },
} as const;
