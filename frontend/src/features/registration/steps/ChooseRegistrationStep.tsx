import { FormControl, FormControlLabel, Radio, RadioGroup, Typography } from "@mui/material";
import type { RegistrationFormData, RegistrationType } from "../types";

type Props = { data: RegistrationFormData; updateData: (updates: Partial<RegistrationFormData>) => void };

export function ChooseRegistrationStep({ data, updateData }: Props) {
  return (
    <FormControl fullWidth>
      <Typography color="text.secondary" sx={{ mb: 2 }}>Start a new family account or add an athlete to an existing family.</Typography>
      <RadioGroup value={data.registrationType} onChange={(event) => updateData({ registrationType: event.target.value as RegistrationType })}>
        <FormControlLabel value="new-family" control={<Radio />} label="Register a brand new family" />
        <FormControlLabel value="existing-family" control={<Radio />} label="Register an athlete to an existing family" />
      </RadioGroup>
    </FormControl>
  );
}
