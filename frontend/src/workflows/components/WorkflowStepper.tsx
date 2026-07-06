import { Box, Step, StepLabel, Stepper } from "@mui/material";
import type { WorkflowStep } from "../types/WorkflowStep";

type WorkflowStepperProps = { steps: WorkflowStep[]; currentStep: number };

export function WorkflowStepper({ steps, currentStep }: WorkflowStepperProps) {
  return (
    <Box sx={{ mb: 4 }}>
      <Stepper activeStep={currentStep} alternativeLabel>
        {steps.map((step) => (
          <Step key={step.id}><StepLabel>{step.title}</StepLabel></Step>
        ))}
      </Stepper>
    </Box>
  );
}
