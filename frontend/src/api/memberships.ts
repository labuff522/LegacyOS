import { http } from './http';

export type MembershipPlan = {
  id: string;
  name: string;
  shortName: string;
  monthlyPrice: number;
  organization: { id: string; name: string; shortName: string };
  services: { id: string; name: string; shortName: string }[];
};

export async function getMembershipPlans(): Promise<MembershipPlan[]> {
  const response = await http.get<MembershipPlan[]>('/membership-plans');
  return response.data;
}
