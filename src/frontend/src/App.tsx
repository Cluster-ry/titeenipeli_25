
import { PropsWithChildren } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";

import GameMap                from "./components/gameMap/GameMap.tsx";
import ApiTestClient          from "./components/ApiClientTest";
import { Welcome }            from "./pages/Welcome/Welcome";
import { Game as GameHolder } from "./pages/Game/Game";

import "./App.css";
import "./assets/PressStart2P-Regular.ttf";


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
        <AppRouter />
    </AppShell>
  );
}

export default App;
