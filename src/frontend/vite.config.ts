import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";
import mkcert from "vite-plugin-mkcert";
import http2Proxy from "vite-plugin-http2-proxy";
import { viteStaticCopy } from "vite-plugin-static-copy";
import * as path from "node:path";

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [
        react(),
        mkcert(),
        http2Proxy({
            "^/Game.StateUpdate/": {
                target: "https://localhost:7222",
                secure: false,
            },
            "^/api": {
                target: "https://localhost:7222",
                secure: false,
            },
        }),
        viteStaticCopy({
            targets: [
                {
                    src: path.resolve(__dirname, "./src/assets"),
                    dest: "./",
                },
            ],
        }),
    ],
    optimizeDeps: {
        include: ["src/generated/**/*.js"],
    },
});
