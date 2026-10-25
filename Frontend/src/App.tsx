import { Outlet } from "react-router";
import Navbar from "./pages/Dashboard/Navbar";

function App() {
  return (
    <>
      <Navbar />
      <main>
        <Outlet />
      </main>
      <footer></footer>
    </>
  );
}

export default App;
