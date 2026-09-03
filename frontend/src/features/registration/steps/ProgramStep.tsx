import { Alert, Card, CardActionArea, CardContent, Chip, CircularProgress, Grid, Stack, Typography } from '@mui/material';
import { useEffect, useState } from 'react';
import { getProducts, type Product } from '../../../api/products';
import type { RegistrationFormData } from '../types';

export function ProgramStep({ data, updateData }: { data: RegistrationFormData; updateData: (updates: Partial<RegistrationFormData>) => void }) {
  const [products, setProducts] = useState<Product[]>([]); const [loading, setLoading] = useState(true); const [error, setError] = useState('');
  useEffect(() => { getProducts().then(items => setProducts(items.filter(x => x.isActive && x.isSessionPackage))).catch(() => setError('Unable to load products.')).finally(() => setLoading(false)); }, []);
  if (loading) return <CircularProgress/>; if (error) return <Alert severity="error">{error}</Alert>;
  return <Grid container spacing={2}>{products.map(product => { const selected = data.productId === product.id; return <Grid key={product.id} size={{ xs: 12, md: 6 }}><Card sx={{ border: selected ? '2px solid #c0c0c0' : '1px solid rgba(255,255,255,0.12)' }}><CardActionArea onClick={() => updateData({ productId: product.id, productName: product.name })}><CardContent><Stack spacing={1}><Stack direction="row" sx={{ justifyContent: 'space-between' }}><Typography variant="h6">{product.name}</Typography>{selected && <Chip label="Selected" size="small"/>}</Stack><Typography color="text.secondary">${product.price.toFixed(2)} · {product.hasUnlimitedSessions ? 'Unlimited sessions' : `${product.sessionCount} sessions`} · valid {product.validityDays} days</Typography><Typography>{product.description}</Typography></Stack></CardContent></CardActionArea></Card></Grid>; })}{products.length === 0 && <Alert severity="warning">Create an active session product before registering athletes.</Alert>}</Grid>;
}
