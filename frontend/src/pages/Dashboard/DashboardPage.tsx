import { Card, CardContent, Grid, Typography } from '@mui/material';
import { PageHeader } from '../../components/common/PageHeader';

const cards = [
  { label: 'Families', value: 'Ready' },
  { label: 'Registration', value: 'Active' },
  { label: 'Memberships', value: 'Seeded' },
  { label: 'Database', value: 'Connected' },
];

export function DashboardPage() {
  return (
    <>
      <PageHeader title="Dashboard" subtitle="LegacyOS admin portal shell is online." />
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
