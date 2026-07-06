import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// Wichtig: Einrichten eines vite-proxy, damit die API-Requests mit relativen Urls während der Entwicklung 
// an die richtige Adresse weitergeleitet werden.
// Ohne diesen Proxy würden die API-Requests an den Vite-Dev-Server gehen, der auf Port 5173 läuft,
// und nicht an die API, die auf Port 8080 läuft. Das würde zu Fehlern führen, da die API-Endpunkte
// nicht gefunden werden können. //  Mit dem Proxy werden alle Anfragen, die mit "/api" oder "/health" 
// beginnen, automatisch an "http://localhost:8080" weitergeleitet, wo die API läuft.
// Dadurch können wir die Admin-Konsole lokal entwickeln und testen, ohne CORS-Probleme zu haben 
// oder die API-URL in den Code ändern zu müssen.
export default defineConfig({
  plugins: [react()],
  base: "/admin/",
  server: {
    proxy: {
      "/api": {
        target: "http://localhost:8080",
        changeOrigin: true
      },
      "/health": {
        target: "http://localhost:8080",
        changeOrigin: true
      }
    }
  }
});