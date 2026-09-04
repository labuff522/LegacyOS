import { useEffect, useState } from 'react';
import { Alert, Box, Button, Card, CardContent, Checkbox, CircularProgress, Container, Divider, FormControl, FormControlLabel, InputLabel, MenuItem, Select, Stack, TextField, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { http } from '../../api/http';
import { useAuth } from '../../features/auth/AuthContext';
import axios from 'axios';

type PortalProfile = {
  guardian: { id: string; firstName: string; lastName: string; email: string };
  family: { id: string; familyName: string; athletes: { id: string; firstName: string; lastName: string; dateOfBirth: string; gender?: string; usaWrestling?: { membershipNumber: string; status: string; expiresOn?: string }; sessionPackages: { id: string; productName: string; isUnlimited: boolean; sessionsRemaining?: number; expiresOn: string }[] }[] };
};
type Catalog = { products: { id: string; name: string; description?: string; price: number; isSessionPackage: boolean; hasUnlimitedSessions: boolean; sessionCount?: number; validityDays?: number; installmentCount?: number; billingDayOfMonth?: number }[] };
type Order = { id: string; itemName: string; status: string; amount: number; currency: string; createdOn: string };

export function PortalHomePage() {
  const auth = useAuth(); const navigate = useNavigate();
  const [profile, setProfile] = useState<PortalProfile | null>(null);
  const [error, setError] = useState('');
  const [catalog, setCatalog] = useState<Catalog | null>(null);
  const [athleteId, setAthleteId] = useState('');
  const [checkoutId, setCheckoutId] = useState('');
  const [discountCode, setDiscountCode] = useState('');
  const [orders, setOrders] = useState<Order[]>([]);
  useEffect(() => {
    Promise.all([http.get<PortalProfile>('/portal/me'), http.get<Catalog>('/portal/purchases/catalog'), http.get<Order[]>('/portal/purchases')])
      .then(([profileResponse, catalogResponse, ordersResponse]) => { setProfile(profileResponse.data); setCatalog(catalogResponse.data); setOrders(ordersResponse.data); setAthleteId(profileResponse.data.family.athletes[0]?.id ?? ''); })
      .catch(() => setError('Unable to load your family or available packages.'));
  }, []);
  async function checkout(body: { productId?: string; athleteId?: string; discountCode?: string }, id: string) {
    setCheckoutId(id); setError('');
    try { const response = await http.post<{ checkoutUrl: string }>('/portal/purchases/checkout', body); window.location.assign(response.data.checkoutUrl); }
    catch (checkoutError) {
      const detail = axios.isAxiosError(checkoutError) ? checkoutError.response?.data?.detail ?? checkoutError.response?.data?.message : null;
      setError(typeof detail === 'string' ? detail : 'Unable to start checkout. Please try again.'); setCheckoutId('');
    }
  }
  async function signOut() { await auth.logout(); navigate('/portal/login', { replace: true }); }
  return <Container maxWidth="md" sx={{ py: 6 }}>
    <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
      <Box><Typography variant="h4">Family portal</Typography><Typography color="text.secondary">Secure access to your DenOS family</Typography></Box>
      <Button onClick={signOut}>Sign out</Button>
    </Stack>
    {error && <Alert severity="error">{error}</Alert>}
    {!profile && !error && <CircularProgress />}
    {profile && catalog && <Stack spacing={3}><Card><CardContent sx={{ p: 4 }}>
      <Typography variant="h5">{profile.family.familyName}</Typography>
      <Typography color="text.secondary" sx={{ mt: 1 }}>{profile.guardian.firstName} {profile.guardian.lastName} · {profile.guardian.email}</Typography>
      <Divider sx={{ my: 3 }} />
      <Typography variant="h6" sx={{ mb: 2 }}>Athletes</Typography>
      <Stack spacing={3}>{profile.family.athletes.map(a => <Box key={a.id}><Typography sx={{ fontWeight: 700 }}>{a.firstName} {a.lastName}</Typography><Typography color="text.secondary">Date of birth: {a.dateOfBirth}{a.gender ? ` · ${a.gender}` : ''}</Typography>{a.sessionPackages.map(p => <Typography key={p.id} color="text.secondary">{p.productName}: {p.isUnlimited ? 'Unlimited' : `${p.sessionsRemaining} sessions remaining`} · expires {new Date(p.expiresOn).toLocaleDateString()}</Typography>)}<UsaWrestlingEntry athlete={a} /></Box>)}</Stack>
      {profile.family.athletes.length === 0 && <Typography color="text.secondary">No athletes are associated with this family.</Typography>}
    </CardContent></Card>
    <AccountSettings currentEmail={profile.guardian.email} onChanged={email => setProfile({ ...profile, guardian: { ...profile.guardian, email } })}/>
    <PortalWaivers />
    <Card><CardContent sx={{ p: 4 }}><Typography variant="h5" sx={{ mb: 3 }}>Products</Typography>
      <FormControl fullWidth sx={{ mb: 3 }}><InputLabel id="athlete-label">Athlete</InputLabel><Select labelId="athlete-label" label="Athlete" value={athleteId} onChange={e => setAthleteId(e.target.value)}>{profile.family.athletes.map(a => <MenuItem key={a.id} value={a.id}>{a.firstName} {a.lastName}</MenuItem>)}</Select></FormControl>
      <TextField fullWidth label="Discount code (optional)" value={discountCode} onChange={e => setDiscountCode(e.target.value.toUpperCase())} sx={{ mb: 3 }}/>
      <Stack spacing={2}>{catalog.products.map(product => <Box key={product.id} sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box><Typography sx={{ fontWeight: 700 }}>{product.name}</Typography><Typography color="text.secondary">{product.description ?? ''} · ${product.price.toFixed(2)}{product.installmentCount ? ` total · ${product.installmentCount} payments of $${(product.price / product.installmentCount).toFixed(2)}${product.billingDayOfMonth ? ` on day ${product.billingDayOfMonth}` : ' monthly from purchase'}` : ''}{product.isSessionPackage ? ` · ${product.hasUnlimitedSessions ? 'Unlimited sessions' : `${product.sessionCount} sessions`} · valid ${product.validityDays} days` : ''}</Typography></Box>
        <Button variant="outlined" disabled={checkoutId === product.id || (product.isSessionPackage && !athleteId)} onClick={() => checkout({ productId: product.id, athleteId: product.isSessionPackage ? athleteId : undefined, discountCode: discountCode.trim() || undefined }, product.id)}>Buy</Button>
      </Box>)}</Stack>
    </CardContent></Card>
    {orders.length > 0 && <Card><CardContent sx={{ p: 4 }}><Typography variant="h5" sx={{ mb: 3 }}>Recent purchases</Typography><Stack spacing={2}>
      {orders.map(order => <Box key={order.id} sx={{ display: 'flex', justifyContent: 'space-between' }}><Box><Typography sx={{ fontWeight: 700 }}>{order.itemName}</Typography><Typography color="text.secondary">{new Date(order.createdOn).toLocaleDateString()} · ${order.amount.toFixed(2)} {order.currency.toUpperCase()}</Typography></Box><Typography>{order.status}</Typography></Box>)}
    </Stack></CardContent></Card>}
    </Stack>}
  </Container>;
}

function AccountSettings({ currentEmail, onChanged }: { currentEmail: string; onChanged: (email: string) => void }) {
  const [email, setEmail] = useState(currentEmail); const [password, setPassword] = useState(''); const [message, setMessage] = useState(''); const [error, setError] = useState('');
  async function save() { setError(''); setMessage(''); try { const response = await http.put<{ email: string }>('/portal/account/email', { newEmail: email, currentPassword: password }); onChanged(response.data.email); setPassword(''); setMessage('Email updated. Use the new email the next time you sign in.'); } catch (saveError) { const data = axios.isAxiosError(saveError) ? saveError.response?.data : null; setError(data?.message ?? data?.errors?.email?.[0] ?? data?.errors?.password?.[0] ?? 'Unable to update email.'); } }
  return <Card><CardContent sx={{ p: 4 }}><Typography variant="h5">Account settings</Typography><Typography color="text.secondary" sx={{ my: 2 }}>Update the email used for login, receipts, and your guardian record.</Typography>{error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}{message && <Alert severity="success" sx={{ mb: 2 }}>{message}</Alert>}<Stack spacing={2}><TextField type="email" label="Login email" value={email} onChange={e => setEmail(e.target.value)}/><TextField type="password" label="Current password" value={password} onChange={e => setPassword(e.target.value)}/><Button variant="outlined" disabled={!email.trim() || !password} onClick={save}>Update email</Button></Stack></CardContent></Card>;
}

type PortalWaiverData = { athletes: { id: string; firstName: string; lastName: string }[]; waivers: { id: string; name: string; version: number; fileName: string; isRequired: boolean; signedAthleteIds: string[] }[] };
function PortalWaivers() {
  const [data, setData] = useState<PortalWaiverData | null>(null); const [signedName, setSignedName] = useState(''); const [athleteId, setAthleteId] = useState(''); const [accepted, setAccepted] = useState(false); const [error, setError] = useState('');
  async function load() { const response = await http.get<PortalWaiverData>('/portal/waivers'); setData(response.data); setAthleteId(current => current || response.data.athletes[0]?.id || ''); }
  useEffect(() => { http.get<PortalWaiverData>('/portal/waivers').then(r => { setData(r.data); setAthleteId(r.data.athletes[0]?.id ?? ''); }).catch(() => setError('Unable to load waivers.')); }, []);
  async function view(id: string, fileName: string) { const response = await http.get(`/portal/waivers/${id}/file`, { responseType: 'blob' }); const url = URL.createObjectURL(response.data); const anchor = document.createElement('a'); anchor.href = url; anchor.target = '_blank'; anchor.download = fileName; anchor.click(); setTimeout(() => URL.revokeObjectURL(url), 30_000); }
  async function sign(id: string) { setError(''); try { await http.post(`/portal/waivers/${id}/sign`, { athleteId, signedName, accepted }); setSignedName(''); setAccepted(false); await load(); } catch { setError('Unable to sign. Confirm the athlete, consent, and typed legal name.'); } }
  if (!data || data.waivers.length === 0) return null;
  return <Card><CardContent sx={{ p: 4 }}><Typography variant="h5">Required documents</Typography>{error && <Alert severity="error" sx={{ my: 2 }}>{error}</Alert>}<FormControl fullWidth sx={{ my: 2 }}><InputLabel>Athlete</InputLabel><Select label="Athlete" value={athleteId} onChange={e => setAthleteId(e.target.value)}>{data.athletes.map(a => <MenuItem key={a.id} value={a.id}>{a.firstName} {a.lastName}</MenuItem>)}</Select></FormControl><Stack spacing={3}>{data.waivers.map(w => { const signed = w.signedAthleteIds.includes(athleteId); return <Box key={w.id}><Typography sx={{ fontWeight: 700 }}>{w.name} · Version {w.version}</Typography><Typography color="text.secondary">{w.isRequired ? 'Required' : 'Optional'} · {signed ? 'Signed' : 'Signature needed'}</Typography><Button sx={{ mt: 1 }} onClick={() => view(w.id, w.fileName)}>Review PDF</Button>{!signed && <Stack spacing={1} sx={{ mt: 2 }}><TextField label="Type your full legal name" value={signedName} onChange={e => setSignedName(e.target.value)}/><FormControlLabel control={<Checkbox checked={accepted} onChange={e => setAccepted(e.target.checked)}/>} label="I have reviewed this waiver and agree to its terms for the selected athlete."/><Button variant="contained" disabled={!athleteId || !accepted || !signedName.trim()} onClick={() => sign(w.id)}>Sign waiver</Button></Stack>}</Box>; })}</Stack></CardContent></Card>;
}

function UsaWrestlingEntry({ athlete }: { athlete: PortalProfile['family']['athletes'][number] }) {
  const [number, setNumber] = useState(athlete.usaWrestling?.membershipNumber ?? '');
  const [status, setStatus] = useState(athlete.usaWrestling?.status ?? 'Not submitted');
  const [saving, setSaving] = useState(false);
  async function submit() {
    setSaving(true);
    try { const response = await http.put<{ status: string }>(`/portal/athletes/${athlete.id}/usa-wrestling-membership`, { membershipNumber: number }); setStatus(response.data.status); }
    finally { setSaving(false); }
  }
  return <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} sx={{ mt: 1, alignItems: { sm: 'center' } }}>
    <TextField size="small" required label="USA Wrestling membership number" value={number} onChange={e => setNumber(e.target.value)} />
    <Button variant="outlined" disabled={saving || number.trim().length < 3} onClick={submit}>Submit for verification</Button>
    <Typography color="text.secondary">Status: {status}</Typography>
  </Stack>;
}
