import { useEffect, useState } from 'react';
import { Alert, Card, CardContent, Chip, CircularProgress, Stack, Typography } from '@mui/material';
import { PageHeader } from '../../components/common/PageHeader';
import { getMembershipPlans, type MembershipPlan } from '../../api/memberships';

export function MembershipsPage() {
  const [plans, setPlans] = useState<MembershipPlan[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getMembershipPlans().then(setPlans).catch(() => setError('Unable to load membership plans. Make sure the API is running.')).finally(() => setLoading(false));
  }, []);

  return (
    <>
      <PageHeader title="Membership Plans" subtitle="Plans, prices, organizations, and included services." />
      {loading && <CircularProgress />}
      {error && <Alert severity="error">{error}</Alert>}
      {!loading && !error && (
        <Stack spacing={2}>
          {plans.map((plan) => (
            <Card key={plan.id}>
              <CardContent>
                <Typography variant="h6">{plan.name}</Typography>
                <Typography color="text.secondary">{plan.organization.name} · ${plan.monthlyPrice}/month</Typography>
                <Stack direction="row" spacing={1} sx={{ mt: 2, flexWrap: 'wrap' }}>
                  {plan.services.map((service) => <Chip key={service.id} label={service.name} size="small" />)}
                </Stack>
              </CardContent>
            </Card>
          ))}
        </Stack>
      )}
    </>
  );
}
