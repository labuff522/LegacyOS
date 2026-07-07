import { Box, Button } from "@mui/material";

type WorkflowButtonsProps = {
  isFirstStep: boolean;
  isLastStep: boolean;
  canContinue: boolean;
  onBack: () => void;
  onNext: () => void;
  onCancel?: () => void;
};

export function WorkflowButtons({
  isFirstStep,
  isLastStep,
  canContinue,
  onBack,
  onNext,
  onCancel,
}: WorkflowButtonsProps) {
  return (
    <Box sx={{ display: "flex", justifyContent: "space-between", mt: 4 }}>
      <Box>
        {!isFirstStep && (
          <Button variant="outlined" onClick={onBack}>
            Back
          </Button>
        )}
      </Box>

      <Box sx={{ display: "flex", gap: 2 }}>
        {onCancel && (
          <Button variant="text" color="inherit" onClick={onCancel}>
            Cancel
          </Button>
        )}

        <Button variant="contained" onClick={onNext} disabled={!canContinue}>
          {isLastStep ? "Finish" : "Continue"}
        </Button>
      </Box>
    </Box>
  );
}