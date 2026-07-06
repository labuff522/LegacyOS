import { createTheme } from '@mui/material/styles';

export const legacyTheme = createTheme({
  palette: {
    mode: 'dark',
    primary: { main: '#c0c0c0' },
    secondary: { main: '#ffffff' },
    background: { default: '#050505', paper: '#111111' },
    text: { primary: '#f5f5f5', secondary: '#c0c0c0' },
  },
  typography: {
    fontFamily: ['Inter', 'Roboto', 'Arial', 'sans-serif'].join(','),
    h4: { fontWeight: 800, letterSpacing: '-0.03em' },
    h5: { fontWeight: 700 },
    h6: { fontWeight: 700 },
    button: { fontWeight: 700, textTransform: 'none' },
  },
  shape: { borderRadius: 14 },
  components: {
    MuiDrawer: { styleOverrides: { paper: { backgroundImage: 'none', borderRight: '1px solid rgba(192,192,192,0.18)' } } },
    MuiAppBar: { styleOverrides: { root: { backgroundImage: 'none', borderBottom: '1px solid rgba(192,192,192,0.18)' } } },
    MuiCard: { styleOverrides: { root: { backgroundImage: 'none', border: '1px solid rgba(192,192,192,0.16)' } } },
  },
});
