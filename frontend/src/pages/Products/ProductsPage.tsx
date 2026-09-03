import { useEffect, useState } from 'react';
import { Alert, Button, Card, CardContent, Checkbox, Dialog, DialogActions, DialogContent, DialogTitle, FormControlLabel, Stack, TextField, Typography } from '@mui/material';
import { PageHeader } from '../../components/common/PageHeader';
import { createProduct, getProducts, updateProduct, type Product, type ProductInput } from '../../api/products';

const blank: ProductInput = { name: '', shortName: '', description: '', productType: 2, price: 0, isSessionPackage: true,
  hasUnlimitedSessions: false, sessionCount: 10, validityDays: 90, isActive: true };

export function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]); const [editing, setEditing] = useState<Product | null | undefined>();
  const [form, setForm] = useState<ProductInput>(blank); const [error, setError] = useState('');
  async function load() { try { setProducts(await getProducts()); } catch { setError('Unable to load products.'); } }
  useEffect(() => { getProducts().then(setProducts).catch(() => setError('Unable to load products.')); }, []);
  function open(product?: Product) { setEditing(product ?? null); setForm(product ? { ...product, productType: 2 } : blank); }
  async function save() { setError(''); try { if (editing) await updateProduct(editing.id, form); else await createProduct(form); setEditing(undefined); await load(); } catch { setError('Unable to save this product. Check all required values.'); } }
  return <><PageHeader title="Products & session packages" subtitle="Create prices, session allowances, and purchase expiration rules." />
    {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}<Button variant="contained" onClick={() => open()}>Create product</Button>
    <Stack spacing={2} sx={{ mt: 3 }}>{products.map(p => <Card key={p.id}><CardContent sx={{ display: 'flex', justifyContent: 'space-between', gap: 2 }}>
      <div><Typography variant="h6">{p.name}</Typography><Typography color="text.secondary">${p.price.toFixed(2)} · {p.isSessionPackage ? `${p.hasUnlimitedSessions ? 'Unlimited sessions' : `${p.sessionCount} sessions`} · valid ${p.validityDays} days` : p.productType} · {p.isActive ? 'Active' : 'Inactive'}</Typography><Typography>{p.description}</Typography></div>
      <Button onClick={() => open(p)}>Manage</Button></CardContent></Card>)}</Stack>
    <Dialog open={editing !== undefined} onClose={() => setEditing(undefined)} fullWidth><DialogTitle>{editing ? 'Manage product' : 'Create product'}</DialogTitle><DialogContent><Stack spacing={2} sx={{ mt: 1 }}>
      <TextField required label="Name" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })}/>
      <TextField required label="Short name" value={form.shortName} onChange={e => setForm({ ...form, shortName: e.target.value })}/>
      <TextField label="Description" value={form.description} onChange={e => setForm({ ...form, description: e.target.value })}/>
      <TextField required type="number" label="Price" value={form.price} onChange={e => setForm({ ...form, price: Number(e.target.value) })}/>
      <FormControlLabel control={<Checkbox checked={form.isSessionPackage} onChange={e => setForm({ ...form, isSessionPackage: e.target.checked })}/>} label="Session package" />
      {form.isSessionPackage && <><FormControlLabel control={<Checkbox checked={form.hasUnlimitedSessions} onChange={e => setForm({ ...form, hasUnlimitedSessions: e.target.checked })}/>} label="Unlimited sessions" />
      {!form.hasUnlimitedSessions && <TextField required type="number" label="Number of sessions" value={form.sessionCount ?? ''} onChange={e => setForm({ ...form, sessionCount: Number(e.target.value) })}/>}<TextField required type="number" label="Valid for days" value={form.validityDays ?? ''} onChange={e => setForm({ ...form, validityDays: Number(e.target.value) })}/></>}
      <FormControlLabel control={<Checkbox checked={form.isActive} onChange={e => setForm({ ...form, isActive: e.target.checked })}/>} label="Available for purchase" />
    </Stack></DialogContent><DialogActions><Button onClick={() => setEditing(undefined)}>Cancel</Button><Button variant="contained" onClick={save}>Save</Button></DialogActions></Dialog>
  </>;
}
