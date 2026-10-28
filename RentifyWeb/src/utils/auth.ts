import { getToken } from "../api/client";

interface Claims {
  exp?: number;
  role?: string | string[];
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"?:
    string | string[];
}

function claims(): Claims | null {
  const token = getToken();
  if (!token) return null;
  try {
    const payload = token.split(".")[1];
    if (!payload) return null;
    const decoded = JSON.parse(
      atob(payload.replace(/-/g, "+").replace(/_/g, "/")),
    ) as Claims;
    return decoded.exp && decoded.exp * 1000 <= Date.now() ? null : decoded;
  } catch {
    return null;
  }
}
export function isAuthenticated(): boolean {
  return claims() !== null;
}
export function isAdmin(): boolean {
  const payload = claims();
  const roles =
    payload?.role ??
    payload?.["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
  return (Array.isArray(roles) ? roles : [roles]).includes("Admin");
}
