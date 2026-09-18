import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    // The React dev server and the API run as two separate processes.
    // Anything starting with /api or /images is forwarded to the backend,
    // so the browser sees a single origin and we need no CORS setup.
    proxy: {
      '/api': 'http://localhost:5080',
      '/images': 'http://localhost:5080',
    },
  },
})
