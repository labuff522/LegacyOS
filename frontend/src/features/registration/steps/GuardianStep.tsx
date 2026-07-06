import { Grid, TextField } from "@mui/material";
import type { RegistrationFormData } from "../types";

type Props = { data: RegistrationFormData; updateData: (updates: Partial<RegistrationFormData>) => void };

export function GuardianStep({ data, updateData }: Props) {
  return (
    <Grid container spacing={2}>
      <Grid size={{ xs: 12, md: 6 }}><TextField label="Guardian First Name" value={data.guardianFirstName} onChange={(event) => updateData({ guardianFirstName: event.target.value })} fullWidth /></Grid>
      <Grid size={{ xs: 12, md: 6 }}><TextField label="Guardian Last Name" value={data.guardianLastName} onChange={(event) => updateData({ guardianLastName: event.target.value })} fullWidth /></Grid>
      <Grid size={{ xs: 12, md: 6 }}><TextField label="Email" value={data.guardianEmail} onChange={(event) => updateData({ guardianEmail: event.target.value })} fullWidth /></Grid>
      <Grid size={{ xs: 12, md: 6 }}><TextField label="Phone" value={data.guardianPhone} onChange={(event) => updateData({ guardianPhone: event.target.value })} fullWidth /></Grid>
    </Grid>
  );
}
