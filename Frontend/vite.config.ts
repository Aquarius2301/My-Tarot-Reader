import react from "@vitejs/plugin-react";
import { fileURLToPath, URL } from "node:url";
import { defineConfig } from "vite";
import { visualizer } from "rollup-plugin-visualizer";

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    visualizer({ open: true, gzipSize: true, filename: "stats.html" }),
  ],
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
          const chunkMap = {
            "vendor-react": ["react", "react-dom", "scheduler"],
            "vendor-react-router": ["react-router", "react-router-dom"],
            "vendor-tanstack": ["@tanstack"],
            "vendor-i18n": ["i18next", "react-i18next"],
            "vendor-axios": ["axios"],
            "vendor-ant-design": ["@ant-design"],
            "vendor-rc-component": ["@rc-component"],
          };

          if (id.includes("node_modules")) {
            for (const [chunkName, deps] of Object.entries(chunkMap)) {
              if (deps.some((dep) => id.includes(`node_modules/${dep}/`))) {
                return chunkName;
              }
            }
          }

          return undefined;
        },
      },
    },
  },
});
