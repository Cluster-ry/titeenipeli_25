import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import ApiTestClient from "./components/ApiClientTest";
import { Welcome } from "./pages/Welcome/Welcome";
import "./App.css";
import "./assets/PressStart2P-Regular.ttf";
import { PropsWithChildren } from "react";

import { Game as GameHolder } from "./pages/Game/Game";
import GameMap from "./components/gameMap/GameMap.tsx";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

const queryClient = new QueryClient();

function AppShell({ children }: PropsWithChildren) {
  return <div className="app-shell">{children}</div>;
}

function AppRouter() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Welcome />} />
        <Route
          path="/game"
          element={
            <GameHolder
              slot={
                <div
                  style={{
                    color: "gray",
                    background: "lightgreen",
                    height: "100%",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                  }}
                >
                  <GameMap />
                </div>
              }
            />
          }
        />
        <Route path="/map" element={<GameMap />} />
        <Route path="/test" element={<ApiTestClient />} />
      </Routes>
    </Router>
  );
}

function App() {
  return (
    <AppShell>
      <QueryClientProvider client={queryClient}>
        <AppRouter />
      </QueryClientProvider>
    </AppShell>
  );
}

export default App;
