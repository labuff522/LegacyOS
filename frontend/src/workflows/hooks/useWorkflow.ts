import { useState } from "react";
import type { WorkflowStep } from "../types/WorkflowStep";

type UseWorkflowOptions = {
  onFinish?: () => void;
};

export function useWorkflow(steps: WorkflowStep[], options?: UseWorkflowOptions) {
  const [currentStep, setCurrentStep] = useState(0);

  const isFirstStep = currentStep === 0;
  const isLastStep = currentStep === steps.length - 1;

  const next = () => {
    if (isLastStep) {
      options?.onFinish?.();
      return;
    }
    setCurrentStep((s) => s + 1);
  };

  const previous = () => {
    if (!isFirstStep) setCurrentStep((s) => s - 1);
  };

  return { step: steps[currentStep], currentStep, totalSteps: steps.length, next, previous, isFirstStep, isLastStep };
}
