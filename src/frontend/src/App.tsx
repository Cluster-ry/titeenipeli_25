import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import React from 'react'
import Authentication from './components/Authentication'
import Map from './components/Map'


function App() {

  return (
      <Router>
        <div>
          <Routes>
            <Route path="/authenticate" element={ <React.Fragment> <Authentication /> </React.Fragment> } />
            <Route path="/map" element={ <React.Fragment> <Map /> </React.Fragment>} />
          </Routes>
        </div>
      </Router>
  )
}

export default App
