import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

export default defineConfig({
  plugins: [vue()],

  build: {
    // Compila a wwwroot/js/dist/
    outDir: resolve(__dirname, '../wwwroot/js/dist'),
    emptyOutDir: true,

    rollupOptions: {
      input: {
        comprador: resolve(__dirname, 'comprador/main.js'),
        vendedor:  resolve(__dirname, 'vendedor/main.js'),
      },
      output: {
        // Nombres de archivo predecibles (sin hash) para referenciarlos desde cshtml
        entryFileNames: '[name].bundle.js',
        chunkFileNames:  '[name].chunk.js',
        assetFileNames:  '[name].[ext]',
      },
    },
  },

  // Para que funcione "npm run dev" con watch en desarrollo
  resolve: {
    alias: {
      '@shared': resolve(__dirname, 'shared'),
    },
  },
})
