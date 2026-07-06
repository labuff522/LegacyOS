import { Box, Typography } from '@mui/material';
import type { ReactNode } from 'react';

type PageHeaderProps = { title: string; subtitle?: string; action?: ReactNode };

export function PageHeader({ title, subtitle, action }: PageHeaderProps) {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 2, mb: 3 }}>
      <Box>
        <Typography variant="h4"sx={{fontWeight: 800, letterSpacing: "-0.03em"}}>{title}</Typography>
        {subtitle && <Typography color="text.secondary" sx={{ mt: 0.5,maxWidth: 700 }}>{subtitle}</Typography>}
      </Box>
      {action}
    </Box>
  );
}
