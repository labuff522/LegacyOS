import {
  Box,
  Divider,
  Drawer,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Typography,
} from "@mui/material";
import DashboardIcon from "@mui/icons-material/Dashboard";
import GroupsIcon from "@mui/icons-material/Groups";
import PersonAddIcon from "@mui/icons-material/PersonAdd";
import InventoryIcon from "@mui/icons-material/Inventory";
import CalendarMonthIcon from "@mui/icons-material/CalendarMonth";
import EventIcon from "@mui/icons-material/Event";
import BusinessIcon from "@mui/icons-material/Business";
import SettingsIcon from "@mui/icons-material/Settings";
import VerifiedUserIcon from "@mui/icons-material/VerifiedUser";
import HowToRegIcon from "@mui/icons-material/HowToReg";
import { NavLink } from "react-router-dom";

const primaryItems = [
  { label: "Dashboard", path: "/dashboard", icon: <DashboardIcon /> },
  { label: "Families", path: "/families", icon: <GroupsIcon /> },
  { label: "New Registration", path: "/registration", icon: <PersonAddIcon /> },
  { label: "USA Wrestling", path: "/usa-wrestling-verifications", icon: <VerifiedUserIcon /> },
];

const catalogItems = [
  { label: "Products", path: "/products", icon: <InventoryIcon /> },
];

const operationsItems = [
  { label: "Check-in", path: "/sessions", icon: <HowToRegIcon /> },
  { label: "Schedule", path: "/schedule", icon: <CalendarMonthIcon /> },
  { label: "Events", path: "/events", icon: <EventIcon /> },
];

const adminItems = [
  { label: "Organizations", path: "/organizations", icon: <BusinessIcon /> },
];

type SidebarProps = {
  drawerWidth: number;
};

function NavSection({
  title,
  items,
}: {
  title?: string;
  items: { label: string; path: string; icon: React.ReactNode }[];
}) {
  return (
    <Box sx={{ mb: 2 }}>
      {title && (
        <Typography
          variant="caption"
          sx={{
            px: 2,
            mb: 1,
            display: "block",
            color: "rgba(255,255,255,0.38)",
            textTransform: "uppercase",
            letterSpacing: "0.08em",
            fontWeight: 700,
          }}
        >
          {title}
        </Typography>
      )}

      <List sx={{ px: 1.5, py: 0 }}>
        {items.map((item) => (
          <ListItemButton
            key={item.path}
            component={NavLink}
            to={item.path}
            sx={{
              borderRadius: 2,
              mb: 0.5,
              color: "text.secondary",
              "&.active": {
                backgroundColor: "rgba(255,255,255,0.08)",
                color: "#fff",
              },
              "&:hover": {
                backgroundColor: "rgba(255,255,255,0.05)",
              },
            }}
          >
            <ListItemIcon sx={{ color: "inherit", minWidth: 40 }}>
              {item.icon}
            </ListItemIcon>
            <ListItemText primary={item.label} />
          </ListItemButton>
        ))}
      </List>
    </Box>
  );
}

export function Sidebar({ drawerWidth }: SidebarProps) {
  return (
    <Drawer
      variant="permanent"
      sx={{
        width: drawerWidth,
        flexShrink: 0,
        "& .MuiDrawer-paper": {
          width: drawerWidth,
          boxSizing: "border-box",
          backgroundColor: "#050505",
          borderRight: "1px solid rgba(255,255,255,0.08)",
        },
      }}
    >
      <Toolbar>
        <Box>
          <Typography
            variant="h4"
            sx={{ fontWeight: 900, letterSpacing: "-0.08em", color: "#fff" }}
          >
            DenOS
          </Typography>

          <Typography variant="body2" sx={{ color: "rgba(255,255,255,0.55)" }}>
            Administration
          </Typography>
        </Box>
      </Toolbar>

      <Divider sx={{ borderColor: "rgba(255,255,255,0.08)", mb: 2 }} />

      <NavSection items={primaryItems} />
      <NavSection title="Catalog" items={catalogItems} />
      <NavSection title="Operations" items={operationsItems} />
      <NavSection title="Administration" items={adminItems} />

      <Box sx={{ flexGrow: 1 }} />

      <Divider sx={{ borderColor: "rgba(255,255,255,0.08)" }} />

      <List sx={{ px: 1.5, py: 2 }}>
        <ListItemButton
          sx={{
            borderRadius: 2,
            color: "text.secondary",
            "&:hover": {
              backgroundColor: "rgba(255,255,255,0.05)",
            },
          }}
        >
          <ListItemIcon sx={{ color: "inherit", minWidth: 40 }}>
            <SettingsIcon />
          </ListItemIcon>
          <ListItemText primary="Settings" />
        </ListItemButton>
      </List>
    </Drawer>
  );
}
