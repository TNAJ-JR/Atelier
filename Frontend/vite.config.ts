import react from '@vitejs/plugin-react'
import { fileURLToPath } from 'node:url'
import { userInfo } from 'node:os'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  // Chaque compte Windows conserve son cache pour éviter les conflits de permissions.
  cacheDir: `node_modules/.vite-${userInfo().username}`,
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
