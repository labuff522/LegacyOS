const API_BASE = "http://localhost:5021";

export type RegisterFamilyRequest = {
  familyName: string;
  organizationShortName: string;
  guardian: {
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
  };
  athletes: {
    firstName: string;
    lastName: string;
    dateOfBirth: string;
    gender: string;
  }[];
};

export type RegisterFamilyResponse = {
  familyId: string;
  familyName: string;
  guardianId: string;
  athleteIds: string[];
};

export async function registerFamily(
  request: RegisterFamilyRequest
): Promise<RegisterFamilyResponse> {
  const response = await fetch(`${API_BASE}/registration/family`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error("Registration failed.");
  }

  return response.json();
}