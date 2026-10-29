import { useState } from "react";
import { Navigate, useNavigate } from "react-router";

import { useForm } from "react-hook-form";
import { yupResolver } from "@hookform/resolvers/yup";

import { LoginSchema } from "../../../schemas/authSchema";
import { useAuth } from "../../../context/AuthContext";
import type { LoginCredentials } from "../../../types/authTypes";

import { Alert, Button, TextField } from "../../../components/ui";

import styles from "./LoginPage.module.scss";

function LoginPage() {
  const navigate = useNavigate();

  const { login, user, isLoading } = useAuth();

  const [apiError, setApiError] = useState<string>("");

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginCredentials>({
    resolver: yupResolver(LoginSchema),
  });

  async function onSubmit(credentials: LoginCredentials): Promise<void> {
    try {
      setApiError("");

      await login(credentials);

      navigate("/profile", { replace: true });
    } catch (loginError) {
      setApiError(
        loginError instanceof Error ? loginError.message : "Impossible de se connecter.",
      );
    }
  }

  if (isLoading) return <p role="status">Chargement de la session…</p>;

  if (user) {
    return <Navigate to="/profile" replace />;
  }

  return (
    <main className={styles.page}>
      <div className={styles.layout}>
        <section className={styles.brandPanel} aria-label="Atelier">
          <span className="t-h2">Atelier</span>

          <div className={styles.introduction}>
            <p className={`${styles.title} t-display`}>
              Catalogue, clients et commandes de l’atelier.
            </p>
            <p className={`${styles.description} t-intro`}>
              Réservé à l’équipe. Les comptes sont créés par un administrateur.
            </p>
          </div>

          <p className={`${styles.locale} t-overline`}>fr-CA · CAD</p>
        </section>

        <section className={styles.formPanel} aria-labelledby="login-title">
          <div className={styles.formContent}>
            <header className="mb-24">
              <h1 id="login-title" className="t-h1 mb-6">
                Connexion
              </h1>
              <p className="text-secondary">
                Vous serez redirigé vers <code className="t-mono">/profile</code>
              </p>
            </header>

            <form
              className="d-flex flex-column gap-20"
              onSubmit={handleSubmit(onSubmit)}
              noValidate
            >
              {apiError && <Alert>{apiError}</Alert>}

              <TextField
                id="email"
                label="Email"
                type="email"
                autoComplete="username"
                error={errors.email?.message}
                {...register("email")}
              />

              <TextField
                id="password"
                label="Mot de passe"
                type="password"
                autoComplete="current-password"
                error={errors.password?.message}
                {...register("password")}
              />

              <Button
                type="submit"
                variant="primary"
                block
                loading={isSubmitting}
                loadingLabel="Connexion…"
              >
                Se connecter
              </Button>
            </form>
          </div>
        </section>
      </div>
    </main>
  );
}

export default LoginPage;
