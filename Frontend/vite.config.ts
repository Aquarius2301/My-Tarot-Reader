import path from "node:path";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(import.meta.dirname, "./src"),
    },
  },
  build: {
    chunkSizeWarningLimit: 600,
    rollupOptions: {
      output: {
        manualChunks(id: string) {
          // 2 files meaning are too big, so split them into two chunks
          if (id.includes("/src/i18n/locales/tarot/vi.tarot"))
            return "tarot-locales-vi";
          if (id.includes("/src/i18n/locales/tarot/en.tarot"))
            return "tarot-locales-en";
          // if (id.includes("/src/i18n/locales/tarot")) return "tarot-locales";
          if (id.includes("node_modules")) {
            if (id.includes("antd")) return "vendor-antd";
            if (id.includes("@ant-design")) return "vendor-ant-design";
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
