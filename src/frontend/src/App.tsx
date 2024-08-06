import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Authentication from "./pages/Authentication";
import Map from "./pages/Map";
import ApiTestClient from "./components/ApiClientTest";
import { Welcome } from "./pages/Welcome/Welcome";
import "./App.css";
import "./assets/PressStart2P-Regular.ttf";

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Welcome />} />
        <Route path="/authenticate" element={<Authentication />} />
        <Route path="/map" element={<Map />} />
        <Route path="/test" element={<ApiTestClient />} />
      </Routes>
    </Router>
  );
}

export default App;
