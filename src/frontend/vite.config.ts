import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";
import mkcert from "vite-plugin-mkcert";
import http2Proxy from "vite-plugin-http2-proxy";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    react(),
    mkcert(),
    http2Proxy({
      "^/Map.MapUpdate/": {
        target: "https://localhost:7222",
      },
      "^/api": {
        target: "https://localhost:7222",
      },
    }),
  ],
  optimizeDeps: {
    include: ["src/generated/**/*.js"],
  },
});
