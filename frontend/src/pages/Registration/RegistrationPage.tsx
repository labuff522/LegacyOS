import { Card, CardContent, Typography } from '@mui/material';
import { PageHeader } from '../../components/common/PageHeader';

export function RegistrationPage() {
  return (
    <>
      <PageHeader title="Register Family" subtitle="This page will become the guided family registration workflow." />
      <Card>
        <CardContent>
          <Typography variant="h6">Coming next</Typography>
          <Typography color="text.secondary" sx={{ mt: 1 }}>
            Family, guardian, athlete, organization, and membership plan registration in one flow.
          </Typography>
        </CardContent>
      </Card>
    </>
  );
}
