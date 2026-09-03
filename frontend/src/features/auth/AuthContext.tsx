/* eslint-disable react-refresh/only-export-components */
import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import { http } from '../../api/http';
import { authStorageKey as storageKey, loadSession, type AuthSession } from './authStorage';

export type { AuthSession } from './authStorage';

type AuthContextValue = {
  session: AuthSession | null;
  login: (email: string, password: string) => Promise<AuthSession>;
  register: (invitationToken: string, email: string, password: string) => Promise<AuthSession>;
  selfRegister: (request: SelfRegisterRequest) => Promise<AuthSession>;
  logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthSession | null>(loadSession);

  const save = (next: AuthSession) => {
    sessionStorage.setItem(storageKey, JSON.stringify(next));
    setSession(next);
    return next;
  };

  const value = useMemo<AuthContextValue>(() => ({
    session,
    async login(email, password) {
      const response = await http.post<AuthSession>('/portal/auth/login', { email, password });
      return save(response.data);
    },
    async register(invitationToken, email, password) {
      const response = await http.post<AuthSession>('/portal/auth/register', { invitationToken, email, password });
      return save(response.data);
    },
    async selfRegister(request) {
      const response = await http.post<AuthSession>('/portal/auth/self-register', request);
      return save(response.data);
    },
    async logout() {
      try { await http.post('/portal/auth/logout'); } finally {
        sessionStorage.removeItem(storageKey);
        setSession(null);
      }
    },
  }), [session]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export type SelfRegisterRequest = { familyName: string; guardianFirstName: string; guardianLastName: string;
  email: string; phone: string; password: string; signedName: string; acceptedWaiverIds: string[];
  athletes: { firstName: string; lastName: string; dateOfBirth: string; gender?: string }[] };

export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) throw new Error('useAuth must be used inside AuthProvider.');
  return value;
}
