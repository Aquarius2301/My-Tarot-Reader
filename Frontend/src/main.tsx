import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "@/App";
import "@/i18n";
import "@/index.css";
import { getVisitorId } from "@/utils";

const root = document.getElementById("root")!;

createRoot(root).render(
  <StrictMode>
    <App />
  </StrictMode>,
);

// Best-effort warm-up: prime the visitor id cache so the axios request
// interceptor (config.api.ts) can attach `X-Device-Id` to the first request,
// but never gate first paint on fingerprinting.
getVisitorId().catch(() => {});
