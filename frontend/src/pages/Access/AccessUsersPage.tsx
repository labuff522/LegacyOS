import { useEffect, useState } from 'react';
import { Alert, Button, Card, CardContent, Chip, Dialog, DialogActions, DialogContent, DialogTitle, Stack, TextField, Typography } from '@mui/material';
import { http } from '../../api/http';
import { PageHeader } from '../../components/common/PageHeader';

type StaffUser = { id: string; email: string; isActive: boolean; createdOn: string };

export function AccessUsersPage() {
  const [users, setUsers] = useState<StaffUser[]>([]);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');
  const [createOpen, setCreateOpen] = useState(false);
  const [resetUser, setResetUser] = useState<StaffUser | null>(null);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [saving, setSaving] = useState(false);

  async function load() { setUsers((await http.get<StaffUser[]>('/staff/access-users')).data); }
  useEffect(() => {
    let current = true;
    void http.get<StaffUser[]>('/staff/access-users')
      .then(response => { if (current) setUsers(response.data); })
      .catch(() => { if (current) setError('Unable to load administrator accounts.'); });
    return () => { current = false; };
  }, []);

  function apiError(reason: unknown) {
    const response = reason as { response?: { data?: { message?: string } } };
    return response.response?.data?.message ?? 'The administrator change could not be saved.';
  }

  async function create() {
    setSaving(true); setError(''); setMessage('');
    try {
      await http.post('/staff/access-users', { email, password });
      setCreateOpen(false); setEmail(''); setPassword(''); setMessage('Administrator created. Share the temporary password securely.'); await load();
    } catch (reason) { setError(apiError(reason)); } finally { setSaving(false); }
  }

  async function resetPassword() {
    if (!resetUser) return;
    setSaving(true); setError(''); setMessage('');
    try {
      await http.put(`/staff/access-users/${resetUser.id}/password`, { password });
      setResetUser(null); setPassword(''); setMessage('Password reset. Existing sessions were signed out.');
    } catch (reason) { setError(apiError(reason)); } finally { setSaving(false); }
  }

  async function setStatus(user: StaffUser) {
    setError(''); setMessage('');
    try {
      await http.put(`/staff/access-users/${user.id}/status`, { isActive: !user.isActive });
      setMessage(user.isActive ? 'Administrator access deactivated.' : 'Administrator access reactivated.'); await load();
    } catch (reason) { setError(apiError(reason)); }
  }

  async function testEmail() {
    setSaving(true); setError(''); setMessage('');
    try { const response = await http.post<{ message: string }>('/staff/email/test'); setMessage(response.data.message); }
    catch (reason) { const response = reason as { response?: { data?: { detail?: string } } }; setError(response.response?.data?.detail ?? 'Email test failed.'); }
    finally { setSaving(false); }
  }

  return <>
    <PageHeader title="Administrator access" subtitle="Create, reset, and deactivate staff sign-in accounts without deleting audit history." />
    {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
    {message && <Alert severity="success" sx={{ mb: 2 }}>{message}</Alert>}
    <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: 'wrap' }}><Button variant="contained" onClick={() => { setPassword(''); setCreateOpen(true); }}>Add administrator</Button><Button variant="outlined" disabled={saving} onClick={testEmail}>Send test email to me</Button></Stack>
    <Stack spacing={2} sx={{ mt: 3 }}>{users.map(user => <Card key={user.id}><CardContent>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ alignItems: { sm: 'center' }, justifyContent: 'space-between' }}>
        <div><Typography variant="h6">{user.email}</Typography><Typography color="text.secondary">Created {new Date(user.createdOn).toLocaleDateString()}</Typography></div>
        <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}><Chip color={user.isActive ? 'success' : 'default'} label={user.isActive ? 'Active' : 'Inactive'} />
          <Button onClick={() => { setPassword(''); setResetUser(user); }}>Reset password</Button>
          <Button color={user.isActive ? 'warning' : 'success'} onClick={() => setStatus(user)}>{user.isActive ? 'Deactivate' : 'Reactivate'}</Button>
        </Stack>
      </Stack>
    </CardContent></Card>)}</Stack>

    <Dialog open={createOpen} onClose={() => !saving && setCreateOpen(false)} fullWidth><DialogTitle>Add administrator</DialogTitle><DialogContent><Stack spacing={2} sx={{ mt: 1 }}>
      <TextField type="email" label="Email" value={email} onChange={event => setEmail(event.target.value)} />
      <TextField type="password" label="Temporary password" helperText="At least 12 characters" value={password} onChange={event => setPassword(event.target.value)} />
    </Stack></DialogContent><DialogActions><Button disabled={saving} onClick={() => setCreateOpen(false)}>Cancel</Button><Button variant="contained" disabled={saving || !email.trim() || password.length < 12} onClick={create}>Create</Button></DialogActions></Dialog>

    <Dialog open={resetUser !== null} onClose={() => !saving && setResetUser(null)} fullWidth><DialogTitle>Reset password</DialogTitle><DialogContent><Stack spacing={2} sx={{ mt: 1 }}>
      <Typography>Set a temporary password for {resetUser?.email}. All existing sessions for this administrator will be signed out.</Typography>
      <TextField type="password" label="New temporary password" helperText="At least 12 characters" value={password} onChange={event => setPassword(event.target.value)} />
    </Stack></DialogContent><DialogActions><Button disabled={saving} onClick={() => setResetUser(null)}>Cancel</Button><Button variant="contained" disabled={saving || password.length < 12} onClick={resetPassword}>Reset</Button></DialogActions></Dialog>
  </>;
}
