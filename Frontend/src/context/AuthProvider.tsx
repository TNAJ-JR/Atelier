import { useEffect, useState } from "react";
import type { ReactNode } from "react";
import { login as loginRequest } from "../API/authApi";
import { endSession, refreshSession } from "../API/apiClient";
import type { LoginCredentials, User } from "../types/authTypes";
import { AuthContext } from "./AuthContext";

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let isMounted = true;
    const handleSessionChanged = (sessionChangedEvent: Event) => {
      if (isMounted) setUser((sessionChangedEvent as CustomEvent<User | null>).detail);
    };
    window.addEventListener("atelier:session", handleSessionChanged);
    // A shared promise prevents duplicate rotation under React StrictMode.
    void refreshSession().catch(() => {
      if (isMounted) setUser(null);
    }).finally(() => {
      if (isMounted) setIsLoading(false);
    });
    return () => {
      isMounted = false;
      window.removeEventListener("atelier:session", handleSessionChanged);
    };
  }, []);

  async function login(credentials: LoginCredentials): Promise<void> {
    const authenticationResponse = await loginRequest(credentials);
    setUser(authenticationResponse.user);
  }

  async function logout(): Promise<void> {
    // Keep the session visible on network failure so server logout can be retried.
    await endSession();
    setUser(null);
  }

  return (
    <AuthContext.Provider value={{ user, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}
