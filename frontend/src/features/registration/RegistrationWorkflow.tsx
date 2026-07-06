import { Alert, Button } from "@mui/material";
import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { WorkflowLayout } from "../../workflows/components/WorkflowLayout";
import { useWorkflow } from "../../workflows/hooks/useWorkflow";
import type { WorkflowStep } from "../../workflows/types/WorkflowStep";
import { defaultRegistrationFormData } from "./registrationDefaults";
import { AthletesStep } from "./steps/AthletesStep";
import { ChooseRegistrationStep } from "./steps/ChooseRegistrationStep";
import { FamilyInformationStep } from "./steps/FamilyInformationStep";
import { GuardianStep } from "./steps/GuardianStep";
import { OrganizationStep } from "./steps/OrganizationStep";
import { ProgramStep } from "./steps/ProgramStep";
import { ReviewStep } from "./steps/ReviewStep";
import type { RegistrationFormData } from "./types";

export function RegistrationWorkflow() {
  const navigate = useNavigate();
  const [data, setData] = useState<RegistrationFormData>(defaultRegistrationFormData);
  const [finished, setFinished] = useState(false);

  const updateData = (updates: Partial<RegistrationFormData>) => {
    setData((current) => ({ ...current, ...updates }));
  };

  const steps: WorkflowStep[] = useMemo(() => [
    { id: "choose-registration", title: "Choose Registration", subtitle: "Start with a brand new family or add an athlete to an existing family.", component: <ChooseRegistrationStep data={data} updateData={updateData} /> },
    { id: "family-information", title: "Family Information", subtitle: "Create the family account.", component: <FamilyInformationStep data={data} updateData={updateData} /> },
    { id: "guardian", title: "Guardian", subtitle: "Add the primary guardian or billing contact.", component: <GuardianStep data={data} updateData={updateData} /> },
    { id: "athletes", title: "Athletes", subtitle: "Add one or more athletes.", component: <AthletesStep data={data} updateData={updateData} /> },
    { id: "organization", title: "Organization", subtitle: "Choose where this registration belongs.", component: <OrganizationStep data={data} updateData={updateData} /> },
    { id: "program", title: "Program", subtitle: "Choose the program for this registration.", component: <ProgramStep data={data} updateData={updateData} /> },
    { id: "review", title: "Review", subtitle: "Confirm everything before creating the registration.", component: <ReviewStep data={data} /> },
  ], [data]);

  const workflow = useWorkflow(steps, { onFinish: () => setFinished(true) });

  if (finished) {
    return (
      <Alert severity="success" action={<Button color="inherit" onClick={() => navigate("/families")}>Back to Families</Button>}>
        Registration workflow completed. API submission comes next.
      </Alert>
    );
  }

  return (
    <WorkflowLayout
      title="New Registration"
      subtitle="Guide a family through registration without exposing the database."
      steps={steps}
      currentStep={workflow.currentStep}
      isFirstStep={workflow.isFirstStep}
      isLastStep={workflow.isLastStep}
      onBack={workflow.previous}
      onNext={workflow.next}
      onCancel={() => navigate("/families")}
    />
  );
}
