import { AppBar, Box, Button, Toolbar, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../features/auth/AuthContext';

type HeaderProps = { drawerWidth: number };

export function Header({ drawerWidth }: HeaderProps) {
  const auth = useAuth();
  const navigate = useNavigate();
  async function signOut() { await auth.logout(); navigate('/portal/login', { replace: true }); }
  return (
    <AppBar position="fixed" elevation={0} sx={{ width: { sm: `calc(100% - ${drawerWidth}px)` }, ml: { sm: `${drawerWidth}px` }, backgroundColor: 'background.paper' }}>
      <Toolbar sx={{ justifyContent: 'space-between' }}>
        <Typography variant="h6" color="text.primary">Dashboard</Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Typography variant="body2" color="text.secondary">v0.3 Preview</Typography>
          <Button onClick={signOut}>Sign out</Button>
        </Box>
      </Toolbar>
    </AppBar>
  );
}
