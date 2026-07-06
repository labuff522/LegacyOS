import { Card, CardContent, Grid, Typography } from '@mui/material';
import { PageHeader } from '../../components/common/PageHeader';

const organizations = [
  { name: 'Wolfpack Wrestling Club', type: 'Non-Profit', description: 'Competitive memberships, scholarships, donations, and club training.' },
  { name: 'The Den Franklin', type: 'Commercial', description: 'After-school programs, camps, clinics, rentals, and facility operations.' },
];

export function OrganizationsPage() {
  return (
    <>
      <PageHeader title="Organizations" subtitle="Operating entities inside the LegacyOS ecosystem." />
      <Grid container spacing={3}>
        {organizations.map((org) => (
          <Grid key={org.name} size={{ xs: 12, md: 6 }}>
            <Card><CardContent><Typography variant="h6">{org.name}</Typography><Typography color="text.secondary">{org.type}</Typography><Typography sx={{ mt: 2 }}>{org.description}</Typography></CardContent></Card>
          </Grid>
        ))}
      </Grid>
    </>
  );
}
