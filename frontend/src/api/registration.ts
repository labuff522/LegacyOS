import { http } from "./http";

export type RegisterFamilyRequest = {
  familyName: string;
  productId: string;
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
    athleteGroupId: string;
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
  const response = await http.post<RegisterFamilyResponse>(
    "/registration/family",
    request
  );

  return response.data;
}
