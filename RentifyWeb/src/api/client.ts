import type { ProblemDetails } from "../types/api";

const baseUrl = (import.meta.env.VITE_API_BASE_URL ?? "").replace(/\/$/, "");
const tokenKey = "rentify.accessToken";

export class ApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly code?: string,
    readonly errors?: Record<string, string[] | string>,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

export function getToken(): string | null {
  return localStorage.getItem(tokenKey);
}
function notifyAuthChanged(): void {
  window.dispatchEvent(new Event("rentify:auth-changed"));
}

export function setToken(token: string): void {
  localStorage.setItem(tokenKey, token);
  notifyAuthChanged();
}
export function clearToken(): void {
  localStorage.removeItem(tokenKey);
  notifyAuthChanged();
}

function errorMessage(problem: ProblemDetails | null, status: number): string {
  if (problem?.errors)
    return Object.values(problem.errors)
      .flatMap((value) => (Array.isArray(value) ? value : [value]))
      .join(" ");
  if (problem?.detail) return problem.detail;
  if (problem?.title) return problem.title;
  if (status === 401) return "Oturumunuz sona ermiş. Lütfen yeniden giriş yapın.";
  if (status === 403)
    return "You do not have permission to perform this action.";
  return `The request failed (${status}). Please try again.`;
}

export async function api<T>(
  path: string,
  options: RequestInit = {},
): Promise<T> {
  const headers = new Headers(options.headers);
  if (options.body && !(options.body instanceof FormData))
    headers.set("Content-Type", "application/json");
  const token = getToken();
  if (token) headers.set("Authorization", `Bearer ${token}`);
  let response: Response;
  try {
    response = await fetch(`${baseUrl}${path}`, { ...options, headers });
  } catch {
    throw new ApiError(
      "Unable to reach Rentify. Check your connection and API configuration.",
      0,
    );
  }
  if (!response.ok) {
    let problem: ProblemDetails | null = null;
    try {
      problem = (await response.json()) as ProblemDetails;
    } catch {
      /* Non-JSON error responses are handled by status. */
    }
    if (response.status === 401 && token) clearToken();
    const message =
      response.status === 401 && !token
        ? "Sign in to search rentals."
        : errorMessage(problem, response.status);
    throw new ApiError(
      message,
      response.status,
      problem?.code,
      problem?.errors,
    );
  }
  if (response.status === 204) return undefined as T;
  return response.json() as Promise<T>;
}

export function errorText(error: unknown): string {
  return error instanceof Error
    ? error.message
    : "Something went wrong. Please try again.";
}
