import { http } from './http';
export type AthleteGroup = { id: string; name: string; description: string; isActive?: boolean; athleteCount?: number };
export async function getAthleteGroups() { return (await http.get<AthleteGroup[]>('/athlete-groups')).data; }
export async function createAthleteGroup(value: { name: string; description: string }) { await http.post('/athlete-groups', value); }
export async function updateAthleteGroup(id: string, value: { name: string; description: string; isActive: boolean }) { await http.put(`/athlete-groups/${id}`, value); }
