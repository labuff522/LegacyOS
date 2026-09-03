import { http } from './http';
import axios from 'axios';
export type StaffWaiver = { id: string; name: string; version: number; fileName: string; isRequired: boolean; isActive: boolean; createdOn: string; signatures: { id: string; athleteId: string; athleteName: string; signedName: string; signedOn: string; expiresOn: string }[] };
export async function getFamilyWaivers(familyId: string) { return (await http.get<StaffWaiver[]>(`/staff/waivers/families/${familyId}`)).data; }
export async function uploadWaiver(familyId: string, value: { organizationId: string; name: string; isRequired: boolean; file: File }) {
  const form = new FormData(); form.append('organizationId', value.organizationId); form.append('name', value.name); form.append('isRequired', String(value.isRequired)); form.append('file', value.file);
  await http.post(`/staff/waivers/families/${familyId}`, form);
}
export async function openStaffWaiver(id: string, fileName: string) { const response = await http.get(`/staff/waivers/${id}/file`, { responseType: 'blob' }); const url = URL.createObjectURL(response.data); const anchor = document.createElement('a'); anchor.href = url; anchor.target = '_blank'; anchor.download = fileName; anchor.click(); setTimeout(() => URL.revokeObjectURL(url), 30_000); }
export type GlobalWaiver = Omit<StaffWaiver, 'signatures'> & { signatureCount: number };
export async function getGlobalWaivers() { return (await http.get<GlobalWaiver[]>('/staff/waivers')).data; }
export async function uploadGlobalWaiver(value: { name: string; isRequired: boolean; file: File }) { const form = new FormData(); form.append('name', value.name); form.append('isRequired', String(value.isRequired)); form.append('file', value.file); await http.post('/staff/waivers', form); }
export function waiverUploadError(error: unknown) { if (!axios.isAxiosError(error)) return 'Upload failed. Try the PDF again.'; const message = error.response?.data?.message ?? error.response?.data?.detail; return typeof message === 'string' ? message : `Upload failed (${error.response?.status ?? 'network error'}).`; }
