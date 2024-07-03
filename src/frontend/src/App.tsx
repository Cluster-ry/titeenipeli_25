import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import Authentication from './pages/Authentication'
import Map from './pages/Map'


function App() {

  return (
      <Router>
        <div>
          <Routes>
            <Route path="/authenticate" element={ <> <Authentication /> </> } />
            <Route path="/map" element={ <> <Map /> </>} />
          </Routes>
        </div>
      </Router>
  )
}

export default App
