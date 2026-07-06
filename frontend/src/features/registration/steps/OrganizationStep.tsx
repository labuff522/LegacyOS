import { FormControl, FormControlLabel, Radio, RadioGroup, Typography } from "@mui/material";
import type { RegistrationFormData } from "../types";

type Props = { data: RegistrationFormData; updateData: (updates: Partial<RegistrationFormData>) => void };

export function OrganizationStep({ data, updateData }: Props) {
  return (
    <FormControl fullWidth>
      <Typography color="text.secondary" sx={{ mb: 2 }}>Choose the organization this registration belongs to.</Typography>
      <RadioGroup value={data.organizationShortName} onChange={(event) => updateData({ organizationShortName: event.target.value })}>
        <FormControlLabel value="Wolfpack" control={<Radio />} label="Wolfpack Wrestling Club" />
        <FormControlLabel value="TheDen" control={<Radio />} label="The Den Franklin" />
      </RadioGroup>
    </FormControl>
  );
}
