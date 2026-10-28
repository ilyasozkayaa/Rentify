import { defineConfig, loadEnv } from "vite";
import vue from "@vitejs/plugin-vue";

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");

  return {
    plugins: [vue()],
    server: {
      proxy: {
        "/api": {
          target: env.VITE_API_PROXY_TARGET || "https://localhost:7246",
          changeOrigin: true,
          secure: false,
        },
      },
    },
  };
});
