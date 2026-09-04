import { Navigate, Route, Routes } from 'react-router-dom';
import { DashboardPage } from './pages/Dashboard/DashboardPage';
import { FamiliesPage } from './pages/Families/FamiliesPage';
import { FamilyDetailPage } from './pages/Families/FamilyDetailPage';
import { RegistrationPage } from './pages/Registration/RegistrationPage';
import { AuthPage } from './pages/Portal/AuthPage';
import { PortalHomePage } from './pages/Portal/PortalHomePage';
import { PurchaseSuccessPage } from './pages/Portal/PurchaseSuccessPage';
import { useAuth } from './features/auth/AuthContext';
import { AdminLayout } from './components/layout/AdminLayout';
import type { ReactNode } from 'react';
import { UsaWrestlingVerificationsPage } from './pages/UsaWrestling/UsaWrestlingVerificationsPage';
import { ProductsPage } from './pages/Products/ProductsPage';
import { SessionsPage } from './pages/Sessions/SessionsPage';
import { SelfRegistrationPage } from './pages/Portal/SelfRegistrationPage';
import { WaiversPage } from './pages/Waivers/WaiversPage';

function RequireRole({ role, children }: { role: 'Customer' | 'Staff'; children: ReactNode }) {
  const { session } = useAuth();
  if (!session) return <Navigate to="/portal/login" replace />;
  if (session.role !== role) return <Navigate to={session.role === 'Customer' ? '/portal' : '/dashboard'} replace />;
  return children;
}

function StaffPage({ children }: { children: ReactNode }) {
  return <RequireRole role="Staff"><AdminLayout>{children}</AdminLayout></RequireRole>;
}

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/portal" replace />} />
      <Route path="/login" element={<Navigate to="/portal/login" replace />} />
      <Route path="/portal/login" element={<AuthPage mode="login" />} />
      <Route path="/portal/register" element={<SelfRegistrationPage />} />
      <Route path="/portal" element={<RequireRole role="Customer"><PortalHomePage /></RequireRole>} />
      <Route path="/portal/purchase/success" element={<RequireRole role="Customer"><PurchaseSuccessPage /></RequireRole>} />
      <Route path="/dashboard" element={<StaffPage><DashboardPage /></StaffPage>} />
      <Route path="/families" element={<StaffPage><FamiliesPage /></StaffPage>} />
      <Route path="/families/:familyId" element={<StaffPage><FamilyDetailPage /></StaffPage>} />
      <Route path="/registration" element={<StaffPage><RegistrationPage /></StaffPage>} />
      <Route path="/products" element={<StaffPage><ProductsPage /></StaffPage>} />
      <Route path="/sessions" element={<StaffPage><SessionsPage /></StaffPage>} />
      <Route path="/waivers" element={<StaffPage><WaiversPage /></StaffPage>} />
      <Route path="/usa-wrestling-verifications" element={<StaffPage><UsaWrestlingVerificationsPage /></StaffPage>} />
      <Route path="*" element={<Navigate to="/portal/login" replace />} />
    </Routes>
  );
}
