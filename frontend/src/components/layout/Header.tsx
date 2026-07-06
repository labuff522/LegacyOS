import { AppBar, Box, Toolbar, Typography } from '@mui/material';

type HeaderProps = { drawerWidth: number };

export function Header({ drawerWidth }: HeaderProps) {
  return (
    <AppBar position="fixed" elevation={0} sx={{ width: { sm: `calc(100% - ${drawerWidth}px)` }, ml: { sm: `${drawerWidth}px` }, backgroundColor: 'background.paper' }}>
      <Toolbar sx={{ justifyContent: 'space-between' }}>
        <Typography variant="h6" color="text.primary">Admin Portal</Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Typography variant="body2" color="text.secondary">LegacyOS Pre-Alpha</Typography>
        </Box>
      </Toolbar>
    </AppBar>
  );
}
