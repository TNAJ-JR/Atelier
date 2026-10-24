import { acceptSession, apiClient } from "./apiClient";
import { LoginResponseSchema } from "../schemas/authSchema";
import type { LoginCredentials, LoginResponse } from "../types/authTypes";

export async function login(
  credentials: LoginCredentials,
): Promise<LoginResponse> {
  const loginResponseBody = await apiClient<unknown>("auth/login", {
    method: "POST",
    body: JSON.stringify(credentials),
  });

  try {
    const authenticationResponse = await LoginResponseSchema.validate(loginResponseBody, { strict: true });
    acceptSession(authenticationResponse);
    return authenticationResponse;
  } catch {
    throw new Error(
      "Réponse de connexion invalide : le serveur doit renvoyer un jeton et un utilisateur.",
    );
  }
}
