import { useEffect, useState } from 'react';
import { Alert, Card, CardContent, CircularProgress, Stack, Typography } from '@mui/material';
import { PageHeader } from '../../components/common/PageHeader';
import { getFamilies, type FamilyListItem } from '../../api/families';

export function FamiliesPage() {
  const [families, setFamilies] = useState<FamilyListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getFamilies().then(setFamilies).catch(() => setError('Unable to load families. Make sure the API is running.')).finally(() => setLoading(false));
  }, []);

  return (
    <>
      <PageHeader title="Families" subtitle="Families, guardians, athletes, and organization relationships." />
      {loading && <CircularProgress />}
      {error && <Alert severity="error">{error}</Alert>}
      {!loading && !error && (
        <Stack spacing={2}>
          {families.map((family) => (
            <Card key={family.id}>
              <CardContent>
                <Typography variant="h6">{family.familyName} Family</Typography>
                <Typography color="text.secondary">{family.organizations.map((o) => o.name).join(', ') || 'No organization assigned'}</Typography>
                <Typography sx={{ mt: 1 }}>{family.guardians.length} guardian(s) · {family.athletes.length} athlete(s)</Typography>
              </CardContent>
            </Card>
          ))}
        </Stack>
      )}
    </>
  );
}
