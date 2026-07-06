import { Box, Drawer, List, ListItemButton, ListItemIcon, ListItemText, Toolbar, Typography } from '@mui/material';
import DashboardIcon from '@mui/icons-material/Dashboard';
import GroupsIcon from '@mui/icons-material/Groups';
import CardMembershipIcon from '@mui/icons-material/CardMembership';
import BusinessIcon from '@mui/icons-material/Business';
import PersonAddIcon from '@mui/icons-material/PersonAdd';
import { NavLink } from 'react-router-dom';

const navItems = [
  { label: 'Dashboard', path: '/dashboard', icon: <DashboardIcon /> },
  { label: 'Families', path: '/families', icon: <GroupsIcon /> },
  { label: 'Registration', path: '/registration', icon: <PersonAddIcon /> },
  { label: 'Memberships', path: '/memberships', icon: <CardMembershipIcon /> },
  { label: 'Organizations', path: '/organizations', icon: <BusinessIcon /> },
];

type SidebarProps = { drawerWidth: number };

export function Sidebar({ drawerWidth }: SidebarProps) {
  return (
    <Drawer variant="permanent" sx={{ width: drawerWidth, flexShrink: 0, [`& .MuiDrawer-paper`]: { width: drawerWidth, boxSizing: 'border-box', backgroundColor: '#050505' } }}>
      <Toolbar>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 900, letterSpacing: '-0.04em' }}>DenOS</Typography>
          <Typography variant="caption" color="text.secondary"> Admin Portal </Typography>
        </Box>
      </Toolbar>
      <List sx={{ px: 1.5 }}>
        {navItems.map((item) => (
          <ListItemButton key={item.path} component={NavLink} to={item.path} sx={{ borderRadius: 2, mb: 0.5, color: 'text.secondary', '&.active': { backgroundColor: 'rgba(192,192,192,0.14)', color: 'text.primary' }, '&:hover': { backgroundColor: 'rgba(192,192,192,0.10)' } }}>
            <ListItemIcon sx={{ color: 'inherit', minWidth: 40 }}>{item.icon}</ListItemIcon>
            <ListItemText primary={item.label} />
          </ListItemButton>
        ))}
      </List>
    </Drawer>
  );
}
