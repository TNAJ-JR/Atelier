import { Outlet } from "react-router";
import Navbar from "./pages/Dashboard/Navbar";
import styles from "./App.module.scss";

function App() {
  return (
    <div className={styles.shell}>
      <Navbar />
      <main className={styles.content}>
        <Outlet />
      </main>
    </div>
  );
}

export default App;
