import { useEffect, useState } from 'react';
import { Alert, Button, Card, CardContent, Checkbox, Dialog, DialogActions, DialogContent, DialogTitle, FormControl, FormControlLabel, InputLabel, MenuItem, Select, Stack, TextField, Typography } from '@mui/material';
import { http } from '../../api/http';
import { PageHeader } from '../../components/common/PageHeader';
import { createProduct, getProducts, updateProduct, type Product, type ProductInput } from '../../api/products';

const blank: ProductInput = { name: '', shortName: '', description: '', productType: 2, price: 0, isSessionPackage: true,
  hasUnlimitedSessions: false, sessionCount: 10, validityDays: 90, installmentCount: undefined, billingDayOfMonth: undefined, isActive: true };

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
      <div><Typography variant="h6">{p.name}</Typography><Typography color="text.secondary">${p.price.toFixed(2)} · {p.installmentCount ? `${p.installmentCount} payments of $${(p.price / p.installmentCount).toFixed(2)}${p.billingDayOfMonth ? ` on day ${p.billingDayOfMonth}` : ' monthly from purchase'}` : 'Pay in full'} · {p.isSessionPackage ? `${p.hasUnlimitedSessions ? 'Unlimited sessions' : `${p.sessionCount} sessions`} · valid ${p.validityDays} days` : p.productType} · {p.isActive ? 'Active' : 'Inactive'}</Typography><Typography>{p.description}</Typography></div>
      <Button onClick={() => open(p)}>Manage</Button></CardContent></Card>)}</Stack>
    <DiscountManager products={products}/>
    <Dialog open={editing !== undefined} onClose={() => setEditing(undefined)} fullWidth><DialogTitle>{editing ? 'Manage product' : 'Create product'}</DialogTitle><DialogContent><Stack spacing={2} sx={{ mt: 1 }}>
      <TextField required label="Name" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })}/>
      <TextField required label="Short name" value={form.shortName} onChange={e => setForm({ ...form, shortName: e.target.value })}/>
      <TextField label="Description" value={form.description} onChange={e => setForm({ ...form, description: e.target.value })}/>
      <TextField required type="number" label="Price" value={form.price} onChange={e => setForm({ ...form, price: Number(e.target.value) })}/>
      <TextField type="number" label="Number of equal payments (leave blank for pay in full)" value={form.installmentCount ?? ''} onChange={e => setForm({ ...form, installmentCount: e.target.value ? Number(e.target.value) : undefined })}/>
      {!!form.installmentCount && form.installmentCount > 1 && <TextField type="number" label="Fixed billing day, 1–28 (leave blank for purchase-date monthly)" value={form.billingDayOfMonth ?? ''} onChange={e => setForm({ ...form, billingDayOfMonth: e.target.value ? Number(e.target.value) : undefined })}/>}
      <FormControlLabel control={<Checkbox checked={form.isSessionPackage} onChange={e => setForm({ ...form, isSessionPackage: e.target.checked })}/>} label="Session package" />
      {form.isSessionPackage && <><FormControlLabel control={<Checkbox checked={form.hasUnlimitedSessions} onChange={e => setForm({ ...form, hasUnlimitedSessions: e.target.checked })}/>} label="Unlimited sessions" />
      {!form.hasUnlimitedSessions && <TextField required type="number" label="Number of sessions" value={form.sessionCount ?? ''} onChange={e => setForm({ ...form, sessionCount: Number(e.target.value) })}/>}<TextField required type="number" label="Valid for days" value={form.validityDays ?? ''} onChange={e => setForm({ ...form, validityDays: Number(e.target.value) })}/></>}
      <FormControlLabel control={<Checkbox checked={form.isActive} onChange={e => setForm({ ...form, isActive: e.target.checked })}/>} label="Available for purchase" />
    </Stack></DialogContent><DialogActions><Button onClick={() => setEditing(undefined)}>Cancel</Button><Button variant="contained" onClick={save}>Save</Button></DialogActions></Dialog>
  </>;
}

type Discount = { id: string; code: string; description?: string; discountType: string; value: number; productId?: string; productName?: string; redemptionCount: number; maxRedemptions?: number; isAutomaticSibling: boolean; siblingStartPosition?: number; siblingEndPosition?: number; isActive: boolean };
function DiscountManager({ products }: { products: Product[] }) {
  const [items, setItems] = useState<Discount[]>([]); const [open, setOpen] = useState(false);
  const [form, setForm] = useState({ code: '', description: '', discountType: 1, value: 10, productId: '', maxRedemptions: '', isActive: true, isAutomaticSibling: false, siblingStartPosition: 2, siblingEndPosition: 4 });
  async function load() { setItems((await http.get<Discount[]>('/discount-codes')).data); }
  useEffect(() => { http.get<Discount[]>('/discount-codes').then(r => setItems(r.data)); }, []);
  async function save() { await http.post('/discount-codes', { ...form, productId: form.productId || null, maxRedemptions: form.maxRedemptions ? Number(form.maxRedemptions) : null }); setOpen(false); await load(); }
  return <Card sx={{ mt: 4 }}><CardContent><Stack direction="row" sx={{ justifyContent: 'space-between', mb: 2 }}><div><Typography variant="h5">Discount codes</Typography><Typography color="text.secondary">Codes may be entered manually or applied automatically to sibling athletes.</Typography></div><Button variant="contained" onClick={() => setOpen(true)}>Create discount</Button></Stack><Stack spacing={1}>{items.map(x => <Typography key={x.id}><b>{x.code}</b> · {x.discountType === 'Percentage' ? `${x.value}%` : `$${x.value}`} · {x.productName ?? 'All products'} · {x.isAutomaticSibling ? `automatic for athletes ${x.siblingStartPosition}–${x.siblingEndPosition}` : 'manual code'} · {x.redemptionCount}{x.maxRedemptions ? `/${x.maxRedemptions}` : ''} used · {x.isActive ? 'Active' : 'Inactive'}</Typography>)}</Stack></CardContent>
    <Dialog open={open} onClose={() => setOpen(false)} fullWidth><DialogTitle>Create discount</DialogTitle><DialogContent><Stack spacing={2} sx={{ mt: 1 }}><TextField required label="Internal code/name" value={form.code} onChange={e => setForm({ ...form, code: e.target.value.toUpperCase() })}/><TextField label="Description" value={form.description} onChange={e => setForm({ ...form, description: e.target.value })}/><FormControlLabel control={<Checkbox checked={form.isAutomaticSibling} onChange={e => setForm({ ...form, isAutomaticSibling: e.target.checked })}/>} label="Apply automatically as a sibling discount" />{form.isAutomaticSibling && <Stack direction="row" spacing={2}><TextField type="number" label="First athlete position" value={form.siblingStartPosition} onChange={e => setForm({ ...form, siblingStartPosition: Number(e.target.value) })}/><TextField type="number" label="Last athlete position" value={form.siblingEndPosition} onChange={e => setForm({ ...form, siblingEndPosition: Number(e.target.value) })}/></Stack>}<FormControl><InputLabel>Type</InputLabel><Select label="Type" value={form.discountType} onChange={e => setForm({ ...form, discountType: Number(e.target.value) })}><MenuItem value={1}>Percentage</MenuItem><MenuItem value={2}>Fixed amount</MenuItem></Select></FormControl><TextField type="number" label="Value" value={form.value} onChange={e => setForm({ ...form, value: Number(e.target.value) })}/><FormControl><InputLabel>Product</InputLabel><Select label="Product" value={form.productId} onChange={e => setForm({ ...form, productId: e.target.value })}><MenuItem value="">All products</MenuItem>{products.map(p => <MenuItem key={p.id} value={p.id}>{p.name}</MenuItem>)}</Select></FormControl><TextField type="number" label="Maximum redemptions (optional)" value={form.maxRedemptions} onChange={e => setForm({ ...form, maxRedemptions: e.target.value })}/></Stack></DialogContent><DialogActions><Button onClick={() => setOpen(false)}>Cancel</Button><Button variant="contained" disabled={!form.code.trim()} onClick={save}>Create</Button></DialogActions></Dialog></Card>;
}
