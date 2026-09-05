import { useState, type FormEvent } from 'react';
import { Alert, Box, Button, Card, CardContent, Container, Link, Stack, TextField, Typography } from '@mui/material';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '../../features/auth/AuthContext';

export function AuthPage({ mode }: { mode: 'login' | 'register' }) {
  const auth = useAuth();
  const navigate = useNavigate();
  const [params] = useSearchParams();
  const [email, setEmail] = useState(params.get('email') ?? '');
  const [password, setPassword] = useState('');
  const [invitationToken, setInvitationToken] = useState(params.get('token') ?? '');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault(); setError(''); setSubmitting(true);
    try {
      const session = mode === 'login'
        ? await auth.login(email, password)
        : await auth.register(invitationToken, email, password);
      navigate(session.role === 'Staff' ? '/dashboard' : '/portal', { replace: true });
    } catch {
      setError(mode === 'login' ? 'Email or password is incorrect.' : 'Unable to create the account. Check the invitation and email.');
    } finally { setSubmitting(false); }
  }

  return <Container maxWidth="sm" sx={{ py: 10 }}>
    <Card><CardContent sx={{ p: 4 }}>
      <Typography variant="h4">DenOS</Typography>
      <Typography color="text.secondary" sx={{ mt: 1, mb: 3 }}>
        {mode === 'login' ? 'Sign in to your family portal.' : 'Create your parent or guardian account.'}
      </Typography>
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      <Box component="form" onSubmit={submit}>
        <Stack spacing={2}>
          {mode === 'register' && <TextField required label="Invitation code" value={invitationToken} onChange={e => setInvitationToken(e.target.value)} />}
          <TextField required type="email" autoComplete="email" label="Email" value={email} onChange={e => setEmail(e.target.value)} />
          <TextField required type="password" autoComplete={mode === 'login' ? 'current-password' : 'new-password'} label="Password" helperText={mode === 'register' ? 'At least 12 characters' : undefined} value={password} onChange={e => setPassword(e.target.value)} slotProps={{ htmlInput: { minLength: mode === 'register' ? 12 : undefined } }} />
          <Button disabled={submitting} type="submit" size="large" variant="contained">{mode === 'login' ? 'Sign in' : 'Create account'}</Button>
          {mode === 'login' && <Link component="button" type="button" onClick={() => navigate('/portal/forgot-password')}>Forgot password?</Link>}
          <Link component="button" type="button" onClick={() => navigate(mode === 'login' ? '/portal/register' : '/portal/login')}>
            {mode === 'login' ? 'New family? Register and choose a package' : 'Already have an account? Sign in'}
          </Link>
        </Stack>
      </Box>
    </CardContent></Card>
  </Container>;
}
