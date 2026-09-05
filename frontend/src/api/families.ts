import { http } from './http';

export type FamilyListItem = {
  id: string;
  familyName: string;
  isActive: boolean;
  organizations: {
    id: string;
    name: string;
    shortName: string;
    isActive: boolean;
    joinedOn: string;
  }[];
  guardians: {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
    isPrimaryContact: boolean;
  }[];
  athletes: {
    id: string;
    firstName: string;
    lastName: string;
    dateOfBirth: string;
    gender?: string;
    sessionPackages?: { id: string; productName: string; isUnlimited: boolean; sessionsRemaining?: number; expiresOn: string }[];
    missingRequiredWaivers?: number;
  }[];
};

export type FamilyDetail = FamilyListItem;

export async function getFamilies(): Promise<FamilyListItem[]> {
  const response = await http.get<FamilyListItem[]>('/families');
  return response.data;
}

export async function getFamily(id: string): Promise<FamilyDetail> {
  const response = await http.get<FamilyDetail>(`/families/${id}`);
  return response.data;
}

export async function updateFamily(id: string, value: { familyName: string; isActive: boolean }) { await http.put(`/families/${id}`, value); }
export async function setFamilyArchived(id: string, archived: boolean) { await http.put(`/families/${id}/archive`, { archived }); }
export async function updateGuardian(familyId: string, guardianId: string, value: Omit<FamilyDetail['guardians'][number], 'id'>) { await http.put(`/families/${familyId}/guardians/${guardianId}`, value); }
export type AthleteInput = { firstName: string; lastName: string; dateOfBirth: string; gender?: string };
export async function saveAthlete(familyId: string, value: AthleteInput, athleteId?: string) {
  if (athleteId) await http.put(`/families/${familyId}/athletes/${athleteId}`, value); else await http.post(`/families/${familyId}/athletes`, value);
}
export type HistoricalOrder = { id: string; kind: string; status: string; originalAmount: number; discountAmount: number; amount: number; currency: string; discountCodeSnapshot?: string; familySnapshotJson: string; athleteSnapshotJson?: string; itemSnapshotJson: string; createdOn: string; completedOn?: string };
export async function getFamilyOrders(familyId: string) { return (await http.get<HistoricalOrder[]>(`/families/${familyId}/orders`)).data; }
export async function reconcileOrder(orderId: string) { return (await http.post<{ status: string; paymentStatus?: string }>(`/staff/purchases/${orderId}/reconcile`)).data; }
export async function refundOrder(orderId: string, reason: string) { return (await http.post<{ status: string }>(`/staff/purchases/${orderId}/refund`, { reason })).data; }
export async function sendParentInvitation(guardianId: string) { return (await http.post<{ message: string }>(`/staff/guardian-invitations`, { guardianId })).data; }
export async function resetParentPassword(guardianId: string, password: string) { await http.put(`/staff/guardians/${guardianId}/password`, { password }); }
