/**
 * Converts seconds to hours, minutes, and seconds.
 * @param seconds the number of seconds to convert
 * @returns an object containing the hours, minutes, and seconds
 */
export function convertSecondsToHours(seconds: number): {
  hours: number;
  minutes: number;
  seconds: number;
} {
  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const remainingSeconds = seconds % 60;
  return { hours, minutes, seconds: remainingSeconds };
}

/**
 * Converts an ISO string to a date and time string in the specified locale.
 * @param isoString the iso string to convert
 * @param locale the language locale to use for formatting
 * @returns an object containing the formatted date and time strings
 */
export function convertISOToDate(
  isoString: string,
  locale: string,
): {
  date: string;
  time: string;
} {
  const date = new Date(isoString);
  return {
    date: date.toLocaleDateString(locale, {
      day: "2-digit",
      month: "short",
      year: "numeric",
    }),

    time: date.toLocaleTimeString(locale, {
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
      hour12: locale === "en-US",
    }),
  };
}
