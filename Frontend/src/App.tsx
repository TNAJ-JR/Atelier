import { Outlet } from "react-router";
import Navbar from "./pages/Dashboard/Navbar";

function App() {
  return (
    <div className="d-flex">
      <Navbar />
      <main className="flex-fill" style={{ minWidth: 0 }}>
        <Outlet />
      </main>
    </div>
  );
}

export default App;
