import { http } from './http';

export type FamilyListItem = {
  id: string;
  familyName: string;
  isActive: boolean;
  organizations: { id: string; name: string; shortName: string; isActive: boolean; joinedOn: string }[];
  guardians: { id: string; firstName: string; lastName: string; email: string; phone: string; isPrimaryContact: boolean }[];
  athletes: { id: string; firstName: string; lastName: string; dateOfBirth: string; gender?: string }[];
};

export async function getFamilies(): Promise<FamilyListItem[]> {
  const response = await http.get<FamilyListItem[]>('/families');
  return response.data;
}
