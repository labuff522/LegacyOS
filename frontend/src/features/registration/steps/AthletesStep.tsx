import { Box, Button, Grid, Stack, TextField, Typography } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import DeleteIcon from "@mui/icons-material/Delete";
import type { RegistrationAthlete, RegistrationFormData } from "../types";

type Props = { data: RegistrationFormData; updateData: (updates: Partial<RegistrationFormData>) => void };

export function AthletesStep({ data, updateData }: Props) {
  const updateAthlete = (index: number, updates: Partial<RegistrationAthlete>) => {
    updateData({ athletes: data.athletes.map((athlete, athleteIndex) => athleteIndex === index ? { ...athlete, ...updates } : athlete) });
  };

  const addAthlete = () => updateData({ athletes: [...data.athletes, { firstName: "", lastName: "", dateOfBirth: "", gender: "Male" }] });
  const removeAthlete = (index: number) => {
    if (data.athletes.length === 1) return;
    updateData({ athletes: data.athletes.filter((_, athleteIndex) => athleteIndex !== index) });
  };

  return (
    <Stack spacing={3}>
      {data.athletes.map((athlete, index) => (
        <Box key={index} sx={{ p: 2, border: "1px solid rgba(255,255,255,0.12)", borderRadius: 2 }}>
          <Stack direction="row" justifyContent="space-between" sx={{ mb: 2 }}>
            <Typography sx={{ fontWeight: 700 }}>Athlete {index + 1}</Typography>
            {data.athletes.length > 1 && <Button size="small" color="error" startIcon={<DeleteIcon />} onClick={() => removeAthlete(index)}>Remove</Button>}
          </Stack>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12, md: 6 }}><TextField label="First Name" value={athlete.firstName} onChange={(event) => updateAthlete(index, { firstName: event.target.value })} fullWidth /></Grid>
            <Grid size={{ xs: 12, md: 6 }}><TextField label="Last Name" value={athlete.lastName} onChange={(event) => updateAthlete(index, { lastName: event.target.value })} fullWidth /></Grid>
            <Grid size={{ xs: 12, md: 6 }}><TextField label="Date of Birth" type="date" value={athlete.dateOfBirth} onChange={(event) => updateAthlete(index, { dateOfBirth: event.target.value })} InputLabelProps={{ shrink: true }} fullWidth /></Grid>
            <Grid size={{ xs: 12, md: 6 }}><TextField label="Gender" value={athlete.gender} onChange={(event) => updateAthlete(index, { gender: event.target.value })} fullWidth /></Grid>
          </Grid>
        </Box>
      ))}
      <Button variant="outlined" startIcon={<AddIcon />} onClick={addAthlete}>Add Athlete</Button>
    </Stack>
  );
}
