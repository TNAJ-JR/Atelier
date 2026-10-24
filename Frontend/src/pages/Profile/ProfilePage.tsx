import { Navigate } from "react-router";
import { useState } from "react";
import { useAuth } from "../../context/AuthContext";
import { Button } from "../../components/ui";
import styles from "./ProfilePage.module.scss";

function ProfilePage() {
  const { user, isLoading, logout } = useAuth();
  const [logoutError, setLogoutError] = useState("");
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  async function handleLogout() {
    setIsLoggingOut(true);
    setLogoutError("");
    try {
      await logout();
    } catch {
      setLogoutError(
        "Déconnexion impossible. Vérifiez votre connexion et réessayez.",
      );
    } finally {
      setIsLoggingOut(false);
    }
  }

  if (isLoading) return <p role="status">Chargement de la session…</p>;

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  return (
    <main className={styles.page}>
      <h1 className="t-h1">Mon profil</h1>

      <section className={styles.card}>
        <p className="t-label">{user.name}</p>
        <p className="text-secondary">{user.email}</p>
      </section>

      <div>
        {logoutError && <p role="alert">{logoutError}</p>}
        <Button
          variant="secondary"
          onClick={() => void handleLogout()}
          disabled={isLoggingOut}
        >
          Se déconnecter
        </Button>
      </div>
    </main>
  );
}

export default ProfilePage;
