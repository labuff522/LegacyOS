import { FormControl, FormControlLabel, Radio, RadioGroup, Typography } from "@mui/material";
import type { RegistrationFormData } from "../types";

type Props = { data: RegistrationFormData; updateData: (updates: Partial<RegistrationFormData>) => void };

const wolfpackPrograms = [
  { shortName: "WolfpackCompetitor", label: "Wolfpack Competitor - $379/month" },
  { shortName: "WolfpackElite", label: "Wolfpack Elite - $479/month" },
];

const denPrograms = [
  { shortName: "DenAfterSchool1Day", label: "The Den After School 1 Day - $149/month" },
  { shortName: "DenAfterSchool2Day", label: "The Den After School 2 Day - $249/month" },
];

export function ProgramStep({ data, updateData }: Props) {
  const programs = data.organizationShortName === "TheDen" ? denPrograms : wolfpackPrograms;
  return (
    <FormControl fullWidth>
      <Typography color="text.secondary" sx={{ mb: 2 }}>Choose the program for this registration.</Typography>
      <RadioGroup value={data.membershipPlanShortName} onChange={(event) => updateData({ membershipPlanShortName: event.target.value })}>
        {programs.map((program) => <FormControlLabel key={program.shortName} value={program.shortName} control={<Radio />} label={program.label} />)}
      </RadioGroup>
    </FormControl>
  );
}
