import AddIcon from "@mui/icons-material/Add";
import DeleteIcon from "@mui/icons-material/Delete";
import { useEffect, useState } from "react";
import {
  Box,
  Button,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import type { RegistrationAthlete, RegistrationFormData } from "../types";
import { http } from "../../../api/http";

type AthletesStepProps = {
  data: RegistrationFormData;
  updateData: (updates: Partial<RegistrationFormData>) => void;
};

export function AthletesStep({ data, updateData }: AthletesStepProps) {
  const [groups, setGroups] = useState<{ id: string; name: string; description: string }[]>([]);
  useEffect(() => { http.get<{ athleteGroups: { id: string; name: string; description: string }[] }>("/portal/auth/registration-options").then(r => setGroups(r.data.athleteGroups)); }, []);
  const updateAthlete = (
    index: number,
    updates: Partial<RegistrationAthlete>
  ) => {
    const athletes = data.athletes.map((athlete, athleteIndex) =>
      athleteIndex === index ? { ...athlete, ...updates } : athlete
    );

    updateData({ athletes });
  };

  const addAthlete = () => {
    updateData({
      athletes: [
        ...data.athletes,
        {
          firstName: "",
          lastName: "",
          dateOfBirth: "",
          gender: "Male",
          athleteGroupId: "",
        },
      ],
    });
  };

  const removeAthlete = (index: number) => {
    if (data.athletes.length === 1) return;

    updateData({
      athletes: data.athletes.filter(
        (_, athleteIndex) => athleteIndex !== index
      ),
    });
  };

  return (
    <Stack spacing={3}>
      {data.athletes.map((athlete, index) => (
        <Box
          key={index}
          sx={{
            p: 2,
            border: "1px solid rgba(255,255,255,0.12)",
            borderRadius: 2,
          }}
        >
          <Box
            sx={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
              mb: 2,
            }}
          >
            <Typography sx={{ fontWeight: 700 }}>
              Athlete {index + 1}
            </Typography>

            {data.athletes.length > 1 && (
              <Button
                size="small"
                color="error"
                startIcon={<DeleteIcon />}
                onClick={() => removeAthlete(index)}
              >
                Remove
              </Button>
            )}
          </Box>

          <Grid container spacing={2}>
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                label="First Name"
                value={athlete.firstName}
                onChange={(event) =>
                  updateAthlete(index, { firstName: event.target.value })
                }
                fullWidth
              />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <FormControl fullWidth required><InputLabel>Group</InputLabel><Select label="Group" value={athlete.athleteGroupId} onChange={event => updateAthlete(index, { athleteGroupId: event.target.value })}>{groups.map(group => <MenuItem key={group.id} value={group.id}><Box><Typography>{group.name}</Typography><Typography variant="caption" color="text.secondary">{group.description}</Typography></Box></MenuItem>)}</Select></FormControl>
            </Grid>

            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                label="Last Name"
                value={athlete.lastName}
                onChange={(event) =>
                  updateAthlete(index, { lastName: event.target.value })
                }
                fullWidth
              />
            </Grid>

            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                label="Date of Birth"
                type="date"
                value={athlete.dateOfBirth}
                onChange={(event) =>
                  updateAthlete(index, { dateOfBirth: event.target.value })
                }
                fullWidth
                slotProps={{
                  inputLabel: {
                    shrink: true,
                  },
                }}
              />
            </Grid>

            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                label="Gender"
                value={athlete.gender}
                onChange={(event) =>
                  updateAthlete(index, { gender: event.target.value })
                }
                fullWidth
              />
            </Grid>
          </Grid>
        </Box>
      ))}

      <Button variant="outlined" startIcon={<AddIcon />} onClick={addAthlete}>
        Add Athlete
      </Button>
    </Stack>
  );
}
