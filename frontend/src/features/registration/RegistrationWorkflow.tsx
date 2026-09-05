import { Alert } from "@mui/material";
import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { registerFamily } from "../../api/registration";
import { WorkflowLayout } from "../../workflows/components/WorkflowLayout";
import { useWorkflow } from "../../workflows/hooks/useWorkflow";
import type { WorkflowStep } from "../../workflows/types/WorkflowStep";
import { defaultRegistrationFormData } from "./registrationDefaults";
import { AthletesStep } from "./steps/AthletesStep";
import { ChooseRegistrationStep } from "./steps/ChooseRegistrationStep";
import { FamilyInformationStep } from "./steps/FamilyInformationStep";
import { GuardianStep } from "./steps/GuardianStep";
import { ProgramStep } from "./steps/ProgramStep";
import { ReviewStep } from "./steps/ReviewStep";
import type { RegistrationFormData } from "./types";

export function RegistrationWorkflow() {
  const navigate = useNavigate();

  const [data, setData] = useState<RegistrationFormData>(
    defaultRegistrationFormData
  );

  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const updateData = (updates: Partial<RegistrationFormData>) => {
    setData((current) => ({ ...current, ...updates }));
  };

  const finishRegistration = async () => {
    setSubmitting(true);
    setSubmitError(null);

    try {
      const response = await registerFamily({
        familyName: data.familyName.trim(),
        productId: data.productId,
        guardian: {
          firstName: data.guardianFirstName.trim(),
          lastName: data.guardianLastName.trim(),
          email: data.guardianEmail.trim(),
          phone: data.guardianPhone.trim(),
        },
        athletes: data.athletes.map((athlete) => ({
          firstName: athlete.firstName.trim(),
          lastName: athlete.lastName.trim(),
          dateOfBirth: athlete.dateOfBirth,
          gender: athlete.gender,
          athleteGroupId: athlete.athleteGroupId,
        })),
      });

      navigate(`/families/${response.familyId}`);
    } catch {
      setSubmitError(
        "Registration failed. Please check the information and try again."
      );
    } finally {
      setSubmitting(false);
    }
  };

  const hasValidAthlete = data.athletes.some(
    (athlete) =>
      athlete.firstName.trim() &&
      athlete.lastName.trim() &&
      athlete.dateOfBirth
  );

  const steps: WorkflowStep[] = useMemo(
    () => [
      {
        id: "choose-registration",
        title: "Choose Registration",
        subtitle:
          "Start with a brand new family or add an athlete to an existing family.",
        component: (
          <ChooseRegistrationStep data={data} updateData={updateData} />
        ),
        canContinue: () => !!data.registrationType,
      },
      {
        id: "family-information",
        title: "Family Information",
        subtitle: "Create the family account.",
        component: (
          <FamilyInformationStep data={data} updateData={updateData} />
        ),
        canContinue: () =>
          data.registrationType === "existing-family" ||
          !!data.familyName.trim(),
      },
      {
        id: "guardian",
        title: "Guardian",
        subtitle: "Add the primary guardian or billing contact.",
        component: <GuardianStep data={data} updateData={updateData} />,
        canContinue: () =>
          !!data.guardianFirstName.trim() &&
          !!data.guardianLastName.trim() &&
          !!data.guardianEmail.trim() &&
          !!data.guardianPhone.trim(),
      },
      {
        id: "athletes",
        title: "Athletes",
        subtitle: "Add one or more athletes.",
        component: <AthletesStep data={data} updateData={updateData} />,
        canContinue: () => hasValidAthlete,
      },
      {
        id: "program",
        title: "Program",
        subtitle: "Choose the program for this registration.",
        component: <ProgramStep data={data} updateData={updateData} />,
        canContinue: () => !!data.productId,
      },
      {
        id: "review",
        title: "Review",
        subtitle: "Confirm everything before creating the registration.",
        component: <ReviewStep data={data} />,
        canContinue: () => !submitting,
      },
    ],
    [data, hasValidAthlete, submitting]
  );

  const workflow = useWorkflow(steps, {
    onFinish: finishRegistration,
  });

  return (
    <>
      {submitError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {submitError}
        </Alert>
      )}

      {submitting && (
        <Alert severity="info" sx={{ mb: 2 }}>
          Creating registration...
        </Alert>
      )}

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
    </>
  );
}
