import { useEffect, useState } from 'react';
import { Alert, Box, Button, Card, CardContent, CircularProgress, Container, Divider, FormControl, InputLabel, MenuItem, Select, Stack, TextField, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { http } from '../../api/http';
import { useAuth } from '../../features/auth/AuthContext';

type PortalProfile = {
  guardian: { id: string; firstName: string; lastName: string; email: string };
  family: { id: string; familyName: string; athletes: { id: string; firstName: string; lastName: string; dateOfBirth: string; gender?: string; usaWrestling?: { membershipNumber: string; status: string; expiresOn?: string } }[] };
};
type Catalog = { membershipPlans: { id: string; name: string; monthlyPrice: number; organizationName: string }[]; products: { id: string; name: string; description?: string; price: number }[] };
type Order = { id: string; itemName: string; status: string; amount: number; currency: string; createdOn: string };

export function PortalHomePage() {
  const auth = useAuth(); const navigate = useNavigate();
  const [profile, setProfile] = useState<PortalProfile | null>(null);
  const [error, setError] = useState('');
  const [catalog, setCatalog] = useState<Catalog | null>(null);
  const [athleteId, setAthleteId] = useState('');
  const [checkoutId, setCheckoutId] = useState('');
  const [orders, setOrders] = useState<Order[]>([]);
  useEffect(() => {
    Promise.all([http.get<PortalProfile>('/portal/me'), http.get<Catalog>('/portal/purchases/catalog'), http.get<Order[]>('/portal/purchases')])
      .then(([profileResponse, catalogResponse, ordersResponse]) => { setProfile(profileResponse.data); setCatalog(catalogResponse.data); setOrders(ordersResponse.data); setAthleteId(profileResponse.data.family.athletes[0]?.id ?? ''); })
      .catch(() => setError('Unable to load your family or available packages.'));
  }, []);
  async function checkout(body: { membershipPlanId?: string; productId?: string; athleteId?: string }, id: string) {
    setCheckoutId(id); setError('');
    try { const response = await http.post<{ checkoutUrl: string }>('/portal/purchases/checkout', body); window.location.assign(response.data.checkoutUrl); }
    catch { setError('Unable to start checkout. Please try again.'); setCheckoutId(''); }
  }
  async function signOut() { await auth.logout(); navigate('/portal/login', { replace: true }); }
  return <Container maxWidth="md" sx={{ py: 6 }}>
    <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
      <Box><Typography variant="h4">Family portal</Typography><Typography color="text.secondary">Secure access to your LegacyOS family</Typography></Box>
      <Button onClick={signOut}>Sign out</Button>
    </Stack>
    {error && <Alert severity="error">{error}</Alert>}
    {!profile && !error && <CircularProgress />}
    {profile && catalog && <Stack spacing={3}><Card><CardContent sx={{ p: 4 }}>
      <Typography variant="h5">{profile.family.familyName}</Typography>
      <Typography color="text.secondary" sx={{ mt: 1 }}>{profile.guardian.firstName} {profile.guardian.lastName} · {profile.guardian.email}</Typography>
      <Divider sx={{ my: 3 }} />
      <Typography variant="h6" sx={{ mb: 2 }}>Athletes</Typography>
      <Stack spacing={3}>{profile.family.athletes.map(a => <Box key={a.id}><Typography sx={{ fontWeight: 700 }}>{a.firstName} {a.lastName}</Typography><Typography color="text.secondary">Date of birth: {a.dateOfBirth}{a.gender ? ` · ${a.gender}` : ''}</Typography><UsaWrestlingEntry athlete={a} /></Box>)}</Stack>
      {profile.family.athletes.length === 0 && <Typography color="text.secondary">No athletes are associated with this family.</Typography>}
    </CardContent></Card>
    <Card><CardContent sx={{ p: 4 }}>
      <Typography variant="h5">Membership packages</Typography>
      <FormControl fullWidth sx={{ my: 3 }}><InputLabel id="athlete-label">Athlete</InputLabel><Select labelId="athlete-label" label="Athlete" value={athleteId} onChange={e => setAthleteId(e.target.value)}>
        {profile.family.athletes.map(a => <MenuItem key={a.id} value={a.id}>{a.firstName} {a.lastName}</MenuItem>)}
      </Select></FormControl>
      <Stack spacing={2}>{catalog.membershipPlans.map(plan => <Box key={plan.id} sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box><Typography sx={{ fontWeight: 700 }}>{plan.name}</Typography><Typography color="text.secondary">{plan.organizationName} · ${plan.monthlyPrice.toFixed(2)}/month</Typography></Box>
        <Button variant="contained" disabled={!athleteId || checkoutId === plan.id} onClick={() => checkout({ membershipPlanId: plan.id, athleteId }, plan.id)}>Subscribe</Button>
      </Box>)}</Stack>
      {catalog.membershipPlans.length === 0 && <Typography color="text.secondary">No membership packages are currently available.</Typography>}
    </CardContent></Card>
    <Card><CardContent sx={{ p: 4 }}><Typography variant="h5" sx={{ mb: 3 }}>Products</Typography>
      <Stack spacing={2}>{catalog.products.map(product => <Box key={product.id} sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box><Typography sx={{ fontWeight: 700 }}>{product.name}</Typography><Typography color="text.secondary">{product.description ?? ''} · ${product.price.toFixed(2)}</Typography></Box>
        <Button variant="outlined" disabled={checkoutId === product.id} onClick={() => checkout({ productId: product.id }, product.id)}>Buy</Button>
      </Box>)}</Stack>
    </CardContent></Card>
    {orders.length > 0 && <Card><CardContent sx={{ p: 4 }}><Typography variant="h5" sx={{ mb: 3 }}>Recent purchases</Typography><Stack spacing={2}>
      {orders.map(order => <Box key={order.id} sx={{ display: 'flex', justifyContent: 'space-between' }}><Box><Typography sx={{ fontWeight: 700 }}>{order.itemName}</Typography><Typography color="text.secondary">{new Date(order.createdOn).toLocaleDateString()} · ${order.amount.toFixed(2)} {order.currency.toUpperCase()}</Typography></Box><Typography>{order.status}</Typography></Box>)}
    </Stack></CardContent></Card>}
    </Stack>}
  </Container>;
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
