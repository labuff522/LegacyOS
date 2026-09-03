import { Alert, Card, CardContent, CircularProgress, Grid, Typography } from '@mui/material';
import { useEffect, useState } from 'react';
import { PageHeader } from '../../components/common/PageHeader';
import { http } from '../../api/http';

type DashboardSummary = {
  families: number;
  athletes: number;
  activeEnrollments: number;
  pendingUsaWrestling: number;
};

export function DashboardPage() {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [error, setError] = useState('');

  useEffect(() => {
    http.get<DashboardSummary>('/staff/dashboard')
      .then(response => setSummary(response.data))
      .catch(() => setError('Unable to load dashboard counts.'));
  }, []);

  const cards = summary ? [
    { label: 'Families', value: summary.families },
    { label: 'Athletes', value: summary.athletes },
    { label: 'Active Enrollments', value: summary.activeEnrollments },
    { label: 'Pending USA Wrestling', value: summary.pendingUsaWrestling },
  ] : [];

  return (
    <>
      <PageHeader title="Dashboard" subtitle="Live operational counts from DenOS." />
      {error && <Alert severity="error">{error}</Alert>}
      {!summary && !error && <CircularProgress />}
      <Grid container spacing={3}>
        {cards.map((card) => (
          <Grid key={card.label} size={{ xs: 12, md: 3 }}>
            <Card><CardContent><Typography color="text.secondary" variant="body2">{card.label}</Typography><Typography variant="h5" sx={{ mt: 1 }}>{card.value}</Typography></CardContent></Card>
          </Grid>
        ))}
      </Grid>
    </>
  );
}
