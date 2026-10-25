import { Outlet } from "react-router";
import Navbar from "./pages/Dashboard/Navbar";

function App() {
  return (
    <div className="d-flex">
      <Navbar />
      <main>
        <Outlet />
      </main>
    </div>
  );
}

export default App;
