export type AuthSession = { accessToken: string; expiresOn: string; email: string; role: string };
export const authStorageKey = 'legacyos.portal.session';

export function loadSession(): AuthSession | null {
  try {
    const value = sessionStorage.getItem(authStorageKey);
    const session = value ? JSON.parse(value) as AuthSession : null;
    return session && new Date(session.expiresOn) > new Date() ? session : null;
  } catch { return null; }
}

export function getAccessToken() { return loadSession()?.accessToken ?? null; }
