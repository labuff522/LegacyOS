import { useEffect, useState } from 'react';
import { Alert, Button, Card, CardContent, Chip, Stack, Typography } from '@mui/material';
import { PageHeader } from '../../components/common/PageHeader';
import { http } from '../../api/http';

type Package = { id: string; productName: string; isUnlimited: boolean; sessionsRemaining?: number; expiresOn: string; isExpired: boolean };
type Athlete = { id: string; firstName: string; lastName: string; familyName: string; packages: Package[] };
export function SessionsPage() {
  const [athletes, setAthletes] = useState<Athlete[]>([]); const [error, setError] = useState(''); const [checking, setChecking] = useState('');
  async function load() { try { setAthletes((await http.get<Athlete[]>('/staff/sessions/roster')).data); } catch { setError('Unable to load session balances.'); } }
  useEffect(() => { http.get<Athlete[]>('/staff/sessions/roster').then(r => setAthletes(r.data)).catch(() => setError('Unable to load session balances.')); }, []);
  async function checkIn(id: string) { setChecking(id); setError(''); try { await http.post(`/staff/sessions/athletes/${id}/check-in`, {}); await load(); } catch { setError('Check-in failed. This athlete may not have an active package.'); } finally { setChecking(''); } }
  return <><PageHeader title="Check-in & session balances" subtitle="Current packages, expiration dates, and manual staff check-in." />{error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
    <Stack spacing={2}>{athletes.map(a => { const active = a.packages.filter(p => !p.isExpired && (p.isUnlimited || (p.sessionsRemaining ?? 0) > 0)); return <Card key={a.id}><CardContent sx={{ display: 'flex', justifyContent: 'space-between', gap: 2 }}><div><Typography variant="h6">{a.firstName} {a.lastName}</Typography><Typography color="text.secondary">{a.familyName}</Typography><Stack direction="row" spacing={1} sx={{ mt: 1, flexWrap: 'wrap' }}>{active.map(p => <Chip key={p.id} label={`${p.productName}: ${p.isUnlimited ? 'Unlimited' : `${p.sessionsRemaining} left`} · expires ${new Date(p.expiresOn).toLocaleDateString()}`} />)}{active.length === 0 && <Chip color="warning" label="No active package" />}</Stack></div><Button variant="contained" disabled={checking === a.id || active.length === 0} onClick={() => checkIn(a.id)}>Check in</Button></CardContent></Card>; })}</Stack></>;
}
