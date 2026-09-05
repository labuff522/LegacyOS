export type RegistrationType = "new-family" | "existing-family";

export type RegistrationAthlete = {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  gender: string;
  athleteGroupId: string;
};

export type RegistrationFormData = {
  registrationType: RegistrationType;
  familyName: string;
  guardianFirstName: string;
  guardianLastName: string;
  guardianEmail: string;
  guardianPhone: string;
  athletes: RegistrationAthlete[];
  productId: string;
  productName: string;
};
