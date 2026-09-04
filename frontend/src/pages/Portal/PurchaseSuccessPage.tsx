import { Alert, Button, Card, CardContent, CircularProgress, Container, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { http } from '../../api/http';

export function PurchaseSuccessPage() {
  const navigate = useNavigate();
  const sessionId = new URLSearchParams(window.location.search).get('session_id');
  const [status, setStatus] = useState(sessionId ? 'Confirming' : 'Missing');
  useEffect(() => { if (sessionId) http.post<{ status: string }>('/portal/purchases/confirm', { sessionId }).then(r => setStatus(r.data.status)).catch(() => setStatus('Error')); }, [sessionId]);
  return <Container maxWidth="sm" sx={{ py: 10 }}><Card><CardContent sx={{ p: 5, textAlign: 'center' }}>
    <Typography variant="h4">Payment received</Typography>
    {status === 'Confirming' && <CircularProgress sx={{ my: 2 }}/>} {status === 'Completed' && <Alert severity="success" sx={{ my: 2 }}>Payment confirmed. Your package is active.</Alert>}{status === 'Pending' && <Alert severity="info" sx={{ my: 2 }}>Stripe has not collected the first payment yet. The package activates after payment.</Alert>}{(status === 'Error' || status === 'Missing') && <Alert severity="warning" sx={{ my: 2 }}>We could not confirm this checkout yet. Staff can review the order status.</Alert>}
    <Button variant="contained" onClick={() => navigate('/portal')}>Return to family portal</Button>
  </CardContent></Card></Container>;
}
