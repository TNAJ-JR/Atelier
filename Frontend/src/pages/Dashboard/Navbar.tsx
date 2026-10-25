import { NavLink } from "react-router";
import styles from "./Navbar.module.scss";
function Navbar() {
  return (
    <aside className={styles.sidebar} aria-label="Navigation latérale">
      <div className={styles.brand}>
        <span className={styles.wordmark}>Atelier</span>
        <span className={styles.version}>V1.0</span>
      </div>
      <nav aria-label="Navigation principale">
        <ul className={styles.menu}>
          <li>
            <NavLink to="/">Tableau de bord</NavLink>
          </li>
          <li>
            <NavLink to="/products">Produits</NavLink>
          </li>
          <li className={styles.active} aria-current="page">
            <NavLink to="/clients">Clients</NavLink>
            <span className={styles.dot} aria-hidden="true" />
          </li>
          <li>
            <NavLink to="/commandes">Commandes</NavLink>
          </li>
        </ul>
      </nav>
      <div className={styles.account}>
        <div className={styles.profile}>
          <span className={styles.avatar} aria-hidden="true">
            MT
          </span>
          <div>
            <p className={styles.name}>Marie Tremblay</p>
            <p className={styles.role}>Staff</p>
          </div>
        </div>
        <button className={styles.logout} type="button">
          Se déconnecter
        </button>
      </div>
    </aside>
  );
}
export default Navbar;
