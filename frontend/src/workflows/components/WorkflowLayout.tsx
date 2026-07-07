import { Card, CardContent, Typography } from "@mui/material";
import { WorkflowButtons } from "./WorkflowButtons";
import { WorkflowStepper } from "./WorkflowStepper";
import type { WorkflowStep } from "../types/WorkflowStep";

type WorkflowLayoutProps = {
  title: string;
  subtitle?: string;
  steps: WorkflowStep[];
  currentStep: number;
  isFirstStep: boolean;
  isLastStep: boolean;
  onBack: () => void;
  onNext: () => void;
  onCancel?: () => void;
};

export function WorkflowLayout({
  title,
  subtitle,
  steps,
  currentStep,
  isFirstStep,
  isLastStep,
  onBack,
  onNext,
  onCancel,
}: WorkflowLayoutProps) {
  const step = steps[currentStep];
  const canContinue = step.canContinue ? step.canContinue() : true;

  return (
    <>
      <Typography variant="h4" sx={{ fontWeight: 800, mb: 0.5 }}>
        {title}
      </Typography>

      {subtitle && (
        <Typography color="text.secondary" sx={{ mb: 4 }}>
          {subtitle}
        </Typography>
      )}

      <Card>
        <CardContent sx={{ p: 4 }}>
          <WorkflowStepper steps={steps} currentStep={currentStep} />

          <Typography variant="h5" sx={{ fontWeight: 700, mb: 1 }}>
            {step.title}
          </Typography>

          {step.subtitle && (
            <Typography color="text.secondary" sx={{ mb: 3 }}>
              {step.subtitle}
            </Typography>
          )}

          {step.component}

          <WorkflowButtons
            isFirstStep={isFirstStep}
            isLastStep={isLastStep}
            canContinue={canContinue}
            onBack={onBack}
            onNext={onNext}
            onCancel={onCancel}
          />
        </CardContent>
      </Card>
    </>
  );
}