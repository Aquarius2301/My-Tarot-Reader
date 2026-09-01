import { fileURLToPath, URL } from "node:url";
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  build: {
    chunkSizeWarningLimit: 600,
    rollupOptions: {
      output: {
        manualChunks(id: string) {
          // Locale tarot (~744KB), seperate chunk to avoid bloating the main bundle
          if (id.includes("/src/i18n/locales/tarot/")) return "tarot-locales";
          if (id.includes("node_modules")) {
            if (id.includes("antd") || id.includes("@ant-design"))
              return "vendor-antd";
            if (id.includes("@tanstack")) return "vendor-tanstack";
            if (id.includes("@fingerprintjs")) return "vendor-fingerprintjs";
            if (id.includes("i18next") || id.includes("react-i18next"))
              return "vendor-i18n";
            if (id.includes("react-router")) return "vendor-react-router";
            if (
              id.includes("node_modules/react/") ||
              id.includes("node_modules/react-dom/") ||
              id.includes("node_modules/scheduler/")
            )
              return "vendor-react";
            if (id.includes("axios")) return "vendor-axios";
            return "vendor-common";
          }
          return undefined;
        },
      },
    },
  },
});
