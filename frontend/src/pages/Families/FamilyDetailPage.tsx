import { useEffect, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Divider,
  Stack,
  Tab,
  Tabs,
  Typography,
} from '@mui/material';
import Grid from '@mui/material/Grid';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import EditIcon from '@mui/icons-material/Edit';
import PersonAddIcon from '@mui/icons-material/PersonAdd';
import PaymentsIcon from '@mui/icons-material/Payments';
import { useNavigate, useParams } from 'react-router-dom';
import { PageHeader } from '../../components/common/PageHeader';
import { getFamily, type FamilyDetail } from '../../api/families';

export function FamilyDetailPage() {
  const { familyId } = useParams();
  const navigate = useNavigate();

  const [family, setFamily] = useState<FamilyDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [tab, setTab] = useState(0);

  useEffect(() => {
    if (!familyId) {
      setError('Family id was not provided.');
      setLoading(false);
      return;
    }

    getFamily(familyId)
      .then(setFamily)
      .catch(() => setError('Unable to load this family.'))
      .finally(() => setLoading(false));
  }, [familyId]);

  if (loading) return <CircularProgress />;
  if (error || !family) return <Alert severity="error">{error ?? 'Family not found.'}</Alert>;

  const primaryGuardian = family.guardians[0];

  return (
    <>
      <PageHeader
        title={family.familyName}
        subtitle={family.organizations.map((o) => o.name).join(', ') || 'No organization assigned'}
        action={
          <Stack direction="row" spacing={1}>
            <Button variant="outlined" startIcon={<ArrowBackIcon />} onClick={() => navigate('/families')}>
              Back
            </Button>
            <Button variant="outlined" startIcon={<EditIcon />}>Edit</Button>
            <Button variant="outlined" startIcon={<PersonAddIcon />}>Register Athlete</Button>
            <Button variant="contained" startIcon={<PaymentsIcon />}>Record Payment</Button>
          </Stack>
        }
      />

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, md: 3 }}>
          <StatCard label="Athletes" value={family.athletes.length.toString()} />
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <StatCard label="Guardians" value={family.guardians.length.toString()} />
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <StatCard label="Organizations" value={family.organizations.length.toString()} />
        </Grid>
        <Grid size={{ xs: 12, md: 3 }}>
          <StatCard label="Status" value={family.isActive ? 'Active' : 'Inactive'} />
        </Grid>
      </Grid>

      <Card>
        <CardContent>
          <Tabs value={tab} onChange={(_, nextTab) => setTab(nextTab)} sx={{ mb: 3 }}>
            <Tab label="Overview" />
            <Tab label="Athletes" />
            <Tab label="Memberships" />
            <Tab label="Billing" />
            <Tab label="Documents" />
            <Tab label="Notes" />
          </Tabs>

          {tab === 0 && (
            <Grid container spacing={3}>
              <Grid size={{ xs: 12, md: 6 }}>
                <Section title="Primary Contact">
                  {primaryGuardian ? (
                    <>
                      <Typography sx={{ fontWeight: 700 }}>
                        {primaryGuardian.firstName} {primaryGuardian.lastName}
                      </Typography>
                      <Typography color="text.secondary">{primaryGuardian.email}</Typography>
                      <Typography color="text.secondary">{primaryGuardian.phone}</Typography>
                    </>
                  ) : (
                    <Typography color="text.secondary">No guardian assigned</Typography>
                  )}
                </Section>
              </Grid>

              <Grid size={{ xs: 12, md: 6 }}>
                <Section title="Organizations">
<Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", gap: 1 }}>                    {family.organizations.length === 0 && (
                      <Typography color="text.secondary">No organization assigned</Typography>
                    )}

                    {family.organizations.map((organization) => (
                      <Chip key={organization.id} label={organization.name} variant="outlined" />
                    ))}
                  </Stack>
                </Section>
              </Grid>

              <Grid size={{ xs: 12, md: 6 }}>
                <Section title="Guardians">
                  {family.guardians.map((guardian) => (
                    <Box key={guardian.id} sx={{ mb: 2 }}>
                      <Typography sx={{ fontWeight: 700 }}>
                        {guardian.firstName} {guardian.lastName}
                        {guardian.isPrimaryContact && (
                          <Chip label="Primary" size="small" variant="outlined" sx={{ ml: 1 }} />
                        )}
                      </Typography>
                      <Typography color="text.secondary">{guardian.email}</Typography>
                      <Typography color="text.secondary">{guardian.phone}</Typography>
                    </Box>
                  ))}
                </Section>
              </Grid>

              <Grid size={{ xs: 12, md: 6 }}>
                <Section title="Athletes">
                  {family.athletes.map((athlete) => (
                    <Box key={athlete.id} sx={{ mb: 2 }}>
                      <Typography sx={{ fontWeight: 700 }}>
                        {athlete.firstName} {athlete.lastName}
                      </Typography>
                      <Typography color="text.secondary">
                        DOB: {athlete.dateOfBirth}
                        {athlete.gender ? ` · ${athlete.gender}` : ''}
                      </Typography>
                    </Box>
                  ))}
                </Section>
              </Grid>
            </Grid>
          )}

          {tab === 1 && <ComingSoon title="Athletes" />}
          {tab === 2 && <ComingSoon title="Memberships" />}
          {tab === 3 && <ComingSoon title="Billing" />}
          {tab === 4 && <ComingSoon title="Documents" />}
          {tab === 5 && <ComingSoon title="Notes" />}
        </CardContent>
      </Card>
    </>
  );
}

function StatCard({ label, value }: { label: string; value: string }) {
  return (
    <Card>
      <CardContent>
        <Typography color="text.secondary" variant="body2">{label}</Typography>
        <Typography variant="h5" sx={{ mt: 1, fontWeight: 800 }}>{value}</Typography>
      </CardContent>
    </Card>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <Box>
      <Typography variant="h6">{title}</Typography>
      <Divider sx={{ my: 2 }} />
      {children}
    </Box>
  );
}

function ComingSoon({ title }: { title: string }) {
  return (
    <Box sx={{ py: 6, textAlign: 'center' }}>
      <Typography variant="h6">{title}</Typography>
      <Typography color="text.secondary" sx={{ mt: 1 }}>
        Coming soon
      </Typography>
    </Box>
  );
}