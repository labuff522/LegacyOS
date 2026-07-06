import { Alert, Stack, TextField } from "@mui/material";
import type { RegistrationFormData } from "../types";

type Props = { data: RegistrationFormData; updateData: (updates: Partial<RegistrationFormData>) => void };

export function FamilyInformationStep({ data, updateData }: Props) {
  if (data.registrationType === "existing-family") {
    return <Alert severity="info">Existing-family search will be added in the next version of this workflow.</Alert>;
  }
  return (
    <Stack spacing={2}>
      <TextField label="Family Name" value={data.familyName} onChange={(event) => updateData({ familyName: event.target.value })} fullWidth />
    </Stack>
  );
}
