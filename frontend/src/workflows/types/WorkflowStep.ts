import type { ReactNode } from "react";

export interface WorkflowStep {
  id: string;
  title: string;
  subtitle?: string;
  component: ReactNode;
  canContinue?: () => boolean;
}