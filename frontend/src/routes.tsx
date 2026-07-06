import { Navigate, Route, Routes } from 'react-router-dom';
import { DashboardPage } from './pages/Dashboard/DashboardPage';
import { FamiliesPage } from './pages/Families/FamiliesPage';
import { FamilyDetailPage } from './pages/Families/FamilyDetailPage';
import { MembershipsPage } from './pages/Memberships/MembershipsPage';
import { OrganizationsPage } from './pages/Organizations/OrganizationsPage';
import { RegistrationPage } from './pages/Registration/RegistrationPage';

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="/dashboard" element={<DashboardPage />} />
      <Route path="/families" element={<FamiliesPage />} />
      <Route path="/families/:familyId" element={<FamilyDetailPage />} />
      <Route path="/registration" element={<RegistrationPage />} />
      <Route path="/memberships" element={<MembershipsPage />} />
      <Route path="/organizations" element={<OrganizationsPage />} />
    </Routes>
  );
}