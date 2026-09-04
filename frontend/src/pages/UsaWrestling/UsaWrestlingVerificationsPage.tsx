import { useEffect, useState } from 'react';
import { Alert, Button, Card, CardContent, CircularProgress, MenuItem, Stack, TextField, Typography } from '@mui/material';
import { PageHeader } from '../../components/common/PageHeader';
import { http } from '../../api/http';

type Verification = { id: string; athleteName: string; familyName: string; membershipNumber: string; status: string; submittedOn: string; expiresOn?: string; staffNotes?: string };

export function UsaWrestlingVerificationsPage() {
  const [rows, setRows] = useState<Verification[] | null>(null); const [error, setError] = useState('');
  const load = () => http.get<Verification[]>('/staff/usa-wrestling-verifications').then(r => setRows(r.data)).catch(() => setError('Unable to load USA Wrestling verifications.'));
  useEffect(() => { void load(); }, []);
  async function review(row: Verification, status: string, expiresOn: string, staffNotes: string) {
    await http.put(`/staff/usa-wrestling-verifications/${row.id}`, { status, expiresOn: expiresOn || null, staffNotes }); await load();
  }
  return <><PageHeader title="USA Wrestling Verification" subtitle="Submitted memberships needing review. Current and expired records are hidden." />
    {error && <Alert severity="error">{error}</Alert>}{!rows && !error && <CircularProgress />}
    <Stack spacing={2}>{rows?.map(row => <VerificationCard key={row.id} row={row} review={review} />)}</Stack>
    {rows?.length === 0 && <Typography color="text.secondary">No membership numbers have been submitted.</Typography>}
  </>;
}

function VerificationCard({ row, review }: { row: Verification; review: (row: Verification, status: string, expiresOn: string, notes: string) => Promise<void> }) {
  const [status, setStatus] = useState(row.status === 'Pending' ? 'Current' : row.status); const [expires, setExpires] = useState(row.expiresOn ?? ''); const [notes, setNotes] = useState(row.staffNotes ?? ''); const [saving, setSaving] = useState(false);
  return <Card><CardContent><Typography variant="h6">{row.athleteName}</Typography><Typography color="text.secondary">{row.familyName} · Membership #{row.membershipNumber} · Submitted {new Date(row.submittedOn).toLocaleDateString()}</Typography>
    <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} sx={{ mt: 2 }}><TextField select label="Decision" value={status} onChange={e => setStatus(e.target.value)} sx={{ minWidth: 150 }}><MenuItem value="Current">Current</MenuItem><MenuItem value="Expired">Expired</MenuItem><MenuItem value="Rejected">Rejected</MenuItem></TextField>
      {status !== 'Current' && <TextField type="date" label="Expiration date" value={expires} onChange={e => setExpires(e.target.value)} slotProps={{ inputLabel: { shrink: true } }} />}<TextField label="Staff notes" value={notes} onChange={e => setNotes(e.target.value)} sx={{ flexGrow: 1 }} />
      <Button variant="contained" disabled={saving} onClick={async () => { setSaving(true); try { await review(row, status, expires, notes); } finally { setSaving(false); } }}>Save verification</Button></Stack>
  </CardContent></Card>;
}
