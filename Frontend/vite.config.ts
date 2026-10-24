import react from '@vitejs/plugin-react'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  css: {
    preprocessorOptions: {
      scss: {
        // Permet `@use "styles/mixins"` depuis n'importe quel module, sans ../../../
        loadPaths: [fileURLToPath(new URL('./src', import.meta.url))],
      },
    },
  },
})
