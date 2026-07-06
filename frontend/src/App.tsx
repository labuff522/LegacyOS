import { AppRoutes } from './routes';
import { AdminLayout } from './components/layout/AdminLayout';

export default function App() {
  return (
    <AdminLayout>
      <AppRoutes />
    </AdminLayout>
  );
}
