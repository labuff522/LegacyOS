import {
  Alert,
  Box,
  Card,
  CardActionArea,
  CardContent,
  Chip,
  CircularProgress,
  Grid,
  Stack,
  Typography,
} from "@mui/material";
import { useEffect, useMemo, useState } from "react";
import { getMembershipPlans, type MembershipPlan } from "../../../api/memberships";
import type { RegistrationFormData } from "../types";

type ProgramStepProps = {
  data: RegistrationFormData;
  updateData: (updates: Partial<RegistrationFormData>) => void;
};

export function ProgramStep({ data, updateData }: ProgramStepProps) {
  const [plans, setPlans] = useState<MembershipPlan[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getMembershipPlans()
      .then(setPlans)
      .catch(() => setError("Unable to load programs. Make sure the API is running."))
      .finally(() => setLoading(false));
  }, []);

  const groupedPlans = useMemo(() => {
    return plans.reduce<Record<string, MembershipPlan[]>>((groups, plan) => {
      const organizationName = plan.organization.name;

      if (!groups[organizationName]) {
        groups[organizationName] = [];
      }

      groups[organizationName].push(plan);
      return groups;
    }, {});
  }, [plans]);

  if (loading) {
    return <CircularProgress />;
  }

  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }

  return (
    <Stack spacing={4}>
      {Object.entries(groupedPlans).map(([organizationName, organizationPlans]) => (
        <Box key={organizationName}>
          <Typography variant="h6" sx={{ mb: 2 }}>
            {organizationName}
          </Typography>

          <Grid container spacing={2}>
            {organizationPlans.map((plan) => {
              const selected = data.membershipPlanShortName === plan.shortName;

              return (
                <Grid key={plan.id} size={{ xs: 12, md: 6 }}>
                  <Card
                    sx={{
                      border: selected
                        ? "2px solid #c0c0c0"
                        : "1px solid rgba(255,255,255,0.12)",
                      backgroundColor: selected
                        ? "rgba(255,255,255,0.08)"
                        : "background.paper",
                    }}
                  >
                    <CardActionArea
                      onClick={() =>
                        updateData({
                          organizationShortName: plan.organization.shortName,
                          membershipPlanShortName: plan.shortName,
                        })
                      }
                    >
                      <CardContent>
                        <Stack spacing={1}>
                          <Box
                            sx={{
                              display: "flex",
                              justifyContent: "space-between",
                              gap: 2,
                            }}
                          >
                            <Typography variant="h6">{plan.name}</Typography>

                            {selected && (
                              <Chip label="Selected" size="small" variant="outlined" />
                            )}
                          </Box>

                          <Typography color="text.secondary">
                            ${plan.monthlyPrice}/month
                          </Typography>

                          <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", gap: 1 }}>
                            {plan.services.map((service) => (
                              <Chip
                                key={service.id}
                                label={service.name}
                                size="small"
                              />
                            ))}
                          </Stack>
                        </Stack>
                      </CardContent>
                    </CardActionArea>
                  </Card>
                </Grid>
              );
            })}
          </Grid>
        </Box>
      ))}
    </Stack>
  );
}