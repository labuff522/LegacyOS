import { http } from './http';
export type StaffWaiver = { id: string; name: string; version: number; fileName: string; isRequired: boolean; isActive: boolean; createdOn: string; organizationName: string; signatures: { id: string; athleteId: string; athleteName: string; signedName: string; signedOn: string; expiresOn: string }[] };
export async function getFamilyWaivers(familyId: string) { return (await http.get<StaffWaiver[]>(`/staff/waivers/families/${familyId}`)).data; }
export async function uploadWaiver(familyId: string, value: { organizationId: string; name: string; isRequired: boolean; file: File }) {
  const form = new FormData(); form.append('organizationId', value.organizationId); form.append('name', value.name); form.append('isRequired', String(value.isRequired)); form.append('file', value.file);
  await http.post(`/staff/waivers/families/${familyId}`, form, { headers: { 'Content-Type': 'multipart/form-data' } });
}
export async function openStaffWaiver(id: string, fileName: string) { const response = await http.get(`/staff/waivers/${id}/file`, { responseType: 'blob' }); const url = URL.createObjectURL(response.data); const anchor = document.createElement('a'); anchor.href = url; anchor.target = '_blank'; anchor.download = fileName; anchor.click(); setTimeout(() => URL.revokeObjectURL(url), 30_000); }
