import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "@/App";
import "@/i18n";
import "@/index.css";
import { GoogleOAuthProvider } from "@react-oauth/google";
import { getVisitorId } from "@/utils";

const root = document.getElementById("root")!;

// Warm the visitor id cache so the axios interceptor can attach `X-Device-Id`
// on the very first request.
getVisitorId().finally(() => {
  createRoot(root).render(
    <GoogleOAuthProvider clientId={import.meta.env.VITE_GOOGLE_CLIENT_ID}>
      <StrictMode>
        <App />
      </StrictMode>
    </GoogleOAuthProvider>,
  );
});
