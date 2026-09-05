import type { RegistrationFormData } from "./types";

export const defaultRegistrationFormData: RegistrationFormData = {
  registrationType: "new-family",
  familyName: "",
  guardianFirstName: "",
  guardianLastName: "",
  guardianEmail: "",
  guardianPhone: "",
  athletes: [{ firstName: "", lastName: "", dateOfBirth: "", gender: "Male", athleteGroupId: "" }],
  productId: "",
  productName: "",
};
