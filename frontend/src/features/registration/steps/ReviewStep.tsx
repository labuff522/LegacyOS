import { Box, Divider, Grid, Stack, Typography } from "@mui/material";
import type { RegistrationFormData } from "../types";

type Props = { data: RegistrationFormData };

export function ReviewStep({ data }: Props) {
  return (
    <Stack spacing={3}>
      <ReviewSection title="Family"><Typography>{data.familyName || "Not entered"}</Typography></ReviewSection>
      <ReviewSection title="Guardian">
        <Typography>{data.guardianFirstName} {data.guardianLastName}</Typography>
        <Typography color="text.secondary">{data.guardianEmail}</Typography>
        <Typography color="text.secondary">{data.guardianPhone}</Typography>
      </ReviewSection>
      <ReviewSection title="Athletes">
        {data.athletes.map((athlete, index) => (
          <Box key={index} sx={{ mb: 1 }}>
            <Typography sx={{ fontWeight: 700 }}>{athlete.firstName} {athlete.lastName}</Typography>
            <Typography color="text.secondary">DOB: {athlete.dateOfBirth || "Not entered"} · {athlete.gender}</Typography>
          </Box>
        ))}
      </ReviewSection>
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 6 }}>
          <ReviewSection title="Organization"><Typography>{data.organizationShortName === "TheDen" ? "The Den Franklin" : "Wolfpack Wrestling Club"}</Typography></ReviewSection>
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <ReviewSection title="Program"><Typography>{data.membershipPlanShortName}</Typography></ReviewSection>
        </Grid>
      </Grid>
    </Stack>
  );
}

function ReviewSection({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <Box>
      <Typography variant="h6">{title}</Typography>
      <Divider sx={{ my: 1.5 }} />
      {children}
    </Box>
  );
}
