import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Authentication from "./pages/Authentication";

import ApiTestClient from "./components/ApiClientTest";
import { Welcome } from "./pages/Welcome/Welcome";
import "./App.css";
import "./assets/PressStart2P-Regular.ttf";
import { PropsWithChildren } from "react";
import { Game as GameHolder } from "./pages/Game/Game";
import Game from "./pages/Game.tsx";
import GameMap from "./components/gameMap/GameMap.tsx";


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
                  Game Slot
                </div>
              }
            />
          }
        />
        <Route path="/authenticate" element={<Authentication />} />
        <Route path="/map" element={<GameMap />} />
        <Route path="/test" element={<ApiTestClient />} />
        <Route path="/game" element={<Game />} />
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
