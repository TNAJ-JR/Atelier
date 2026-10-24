import { LoginResponseSchema } from "../schemas/authSchema";
import type { LoginResponse } from "../types/authTypes";

const API_URL = import.meta.env.VITE_API_URL ?? "https://localhost:7262/api";
let accessToken: string | null = null;
let csrfToken: string | null = null;
let pendingCsrfTokenRequest: Promise<string> | null = null;
let pendingSessionRefresh: Promise<LoginResponse> | null = null;
const sessionBroadcastChannel = typeof BroadcastChannel !== "undefined"
  ? new BroadcastChannel("atelier-auth") : null;

// Remove credentials persisted by the previous implementation.
localStorage.removeItem("token");
localStorage.removeItem("user");
localStorage.removeItem("refreshToken");

export function acceptSession(authenticationResponse: LoginResponse): void {
  accessToken = authenticationResponse.token;
  window.dispatchEvent(new CustomEvent("atelier:session", { detail: authenticationResponse.user }));
}

export function clearSession(broadcastLogout = false): void {
  accessToken = null;
  window.dispatchEvent(new CustomEvent("atelier:session", { detail: null }));
  if (broadcastLogout) sessionBroadcastChannel?.postMessage("logout");
}

sessionBroadcastChannel?.addEventListener("message", (event: MessageEvent<unknown>) => {
  if (event.data === "logout") clearSession();
});

async function withCookieLock<T>(cookieOperation: () => Promise<T>): Promise<T> {
  // The refresh cookie is shared by tabs. Web Locks serialize rotation across tabs.
  if (navigator.locks) return navigator.locks.request("atelier-auth-cookie", cookieOperation);
  return cookieOperation();
}

async function getCsrfToken(): Promise<string> {
  if (csrfToken) return csrfToken;
  pendingCsrfTokenRequest ??= fetch(`${API_URL}/auth/csrf`, {
    credentials: "include", cache: "no-store",
  }).then(async (response) => {
    if (!response.ok) throw new Error("Impossible de préparer la protection de la connexion.");
    const csrfResponseBody: unknown = await response.json();
    if (!csrfResponseBody || typeof csrfResponseBody !== "object" || !("csrfToken" in csrfResponseBody) ||
        typeof csrfResponseBody.csrfToken !== "string") throw new Error("Réponse CSRF invalide.");
    csrfToken = csrfResponseBody.csrfToken;
    return csrfToken;
  }).finally(() => { pendingCsrfTokenRequest = null; });
  return pendingCsrfTokenRequest;
}

async function sendApiRequest(endpoint: string, requestOptions: RequestInit, bearerToken: string | null,
  requiresCsrfValidation = false): Promise<Response> {
  const headers = new Headers(requestOptions.headers);
  if (requestOptions.body && !headers.has("Content-Type")) headers.set("Content-Type", "application/json");
  if (bearerToken) headers.set("Authorization", `Bearer ${bearerToken}`);
  else headers.delete("Authorization");
  if (requiresCsrfValidation) headers.set("X-CSRF-TOKEN", await getCsrfToken());
  return fetch(`${API_URL}/${endpoint}`, {
    ...requestOptions, headers, credentials: "include", cache: "no-store",
  });
}

async function sendCookieAuthenticatedRequest(endpoint: string, requestOptions: RequestInit): Promise<Response> {
  let response = await sendApiRequest(endpoint, requestOptions, null, true);
  if (response.status === 400) {
    const errorResponseBody = await response.clone().json().catch(() => null) as { code?: string } | null;
    // Antiforgery rejects before the action: one retry cannot consume a refresh token.
    if (errorResponseBody?.code === "csrf_invalid") {
      csrfToken = null;
      response = await sendApiRequest(endpoint, requestOptions, null, true);
    }
  }
  return response;
}

async function decodeApiResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const errorResponseBody = await response.json().catch(() => null) as { message?: string; title?: string } | null;
    throw new Error(errorResponseBody?.message ?? errorResponseBody?.title ?? "Une erreur est survenue lors de la requête.");
  }
  if (response.status === 204) return null as T;
  return response.json() as Promise<T>;
}

export function refreshSession(): Promise<LoginResponse> {
  pendingSessionRefresh ??= withCookieLock(async () => {
    const response = await sendCookieAuthenticatedRequest("auth/refresh", { method: "POST" });
    if (response.status === 401) clearSession();
    const authenticationResponse = await LoginResponseSchema.validate(await decodeApiResponse<unknown>(response), { strict: true });
    acceptSession(authenticationResponse);
    return authenticationResponse;
  }).finally(() => { pendingSessionRefresh = null; });
  return pendingSessionRefresh;
}

export async function endSession(): Promise<void> {
  await withCookieLock(async () => {
    await decodeApiResponse<void>(await sendCookieAuthenticatedRequest("auth/logout", { method: "POST" }));
    clearSession(true);
  });
}

export async function apiClient<T>(endpoint: string, requestOptions: RequestInit = {}): Promise<T> {
  const endpointPath = endpoint.replace(/^\/+/, "");
  if (endpointPath === "auth/login") {
    return withCookieLock(async () => decodeApiResponse<T>(await sendCookieAuthenticatedRequest(endpointPath, requestOptions)));
  }
  const initialAccessToken = accessToken;
  let response = await sendApiRequest(endpointPath, requestOptions, initialAccessToken);
  if (response.status === 401) {
    if (!accessToken || accessToken === initialAccessToken) await refreshSession();
    response = await sendApiRequest(endpointPath, requestOptions, accessToken);
    if (response.status === 401) clearSession();
  }
  return decodeApiResponse<T>(response);
}
