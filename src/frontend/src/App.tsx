import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Authentication from "./pages/Authentication";
import Map from "./pages/Map";
import ApiTestClient from "./components/ApiClientTest";

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/authenticate" element={<Authentication />} />
        <Route path="/map" element={<Map />} />
        <Route path="/test" element={<ApiTestClient />}/>
      </Routes>
    </Router>
  );
}

export default App;
