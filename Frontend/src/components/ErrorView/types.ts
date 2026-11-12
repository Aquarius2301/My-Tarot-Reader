export type ErrorType = "offline" | "server" | "timeout" | "generic";

export interface ErrorProps {
  /** Explicit error type. If omitted the component auto-detects via navigator.onLine. */
  type?: ErrorType;
  /** Called when the user presses the retry button. If omitted the button is hidden. */
  onRetry?: () => void;
}
