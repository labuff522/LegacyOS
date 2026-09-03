import { Button, Card, CardContent, Container, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';

export function PurchaseSuccessPage() {
  const navigate = useNavigate();
  return <Container maxWidth="sm" sx={{ py: 10 }}><Card><CardContent sx={{ p: 5, textAlign: 'center' }}>
    <Typography variant="h4">Payment received</Typography>
    <Typography color="text.secondary" sx={{ my: 2 }}>Stripe is confirming your purchase. Your package appears after the verified payment webhook is processed.</Typography>
    <Button variant="contained" onClick={() => navigate('/portal')}>Return to family portal</Button>
  </CardContent></Card></Container>;
}
