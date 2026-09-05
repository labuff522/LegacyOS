import { useState, type FormEvent } from 'react';
import { Alert, Box, Button, Card, CardContent, Container, Link, Stack, TextField, Typography } from '@mui/material';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { http } from '../../api/http';

export function PasswordResetPage() {
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const token = params.get('token') ?? '';
  const [email, setEmail] = useState(params.get('email') ?? '');
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault(); setSaving(true); setError('');
    try {
      if (token) {
        await http.post('/portal/auth/reset-password', { email, token, password });
        setMessage('Your password has been reset. You can now sign in.');
      } else {
        const response = await http.post<{ message: string }>('/portal/auth/forgot-password', { email });
        setMessage(response.data.message);
      }
    } catch { setError(token ? 'This reset link is invalid or expired.' : 'Password recovery email is temporarily unavailable.'); }
    finally { setSaving(false); }
  }

  return <Container maxWidth="sm" sx={{ py: 10 }}><Card><CardContent sx={{ p: 4 }}>
    <Typography variant="h4">DenOS</Typography><Typography color="text.secondary" sx={{ mt: 1, mb: 3 }}>{token ? 'Choose a new password.' : 'Request a password reset link.'}</Typography>
    {message && <Alert severity="success" sx={{ mb: 2 }}>{message}</Alert>}{error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
    <Box component="form" onSubmit={submit}><Stack spacing={2}>
      <TextField required type="email" label="Email" value={email} onChange={event => setEmail(event.target.value)} />
      {token && <TextField required type="password" label="New password" helperText="At least 12 characters" value={password} onChange={event => setPassword(event.target.value)} slotProps={{ htmlInput: { minLength: 12 } }} />}
      <Button type="submit" variant="contained" disabled={saving || (Boolean(token) && password.length < 12)}>{token ? 'Reset password' : 'Email reset link'}</Button>
      <Link component="button" type="button" onClick={() => navigate('/portal/login')}>Return to sign in</Link>
    </Stack></Box>
  </CardContent></Card></Container>;
}
