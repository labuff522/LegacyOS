import { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  TextField,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { DataGrid, type GridColDef } from '@mui/x-data-grid';
import { useNavigate } from 'react-router-dom';
import { PageHeader } from '../../components/common/PageHeader';
import { getFamilies, type FamilyListItem } from '../../api/families';

export function FamiliesPage() {
  const navigate = useNavigate();

  const [families, setFamilies] = useState<FamilyListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');

  useEffect(() => {
    getFamilies()
      .then(setFamilies)
      .catch(() => setError('Unable to load families. Make sure the API is running.'))
      .finally(() => setLoading(false));
  }, []);

  const rows = useMemo(() => {
    return families
      .map((family) => {
        const primaryGuardian = family.guardians[0];
        const organizationNames = family.organizations.map((o) => o.name).join(', ');

        return {
          id: family.id,
          familyName: family.familyName,
          organization: organizationNames || 'Unassigned',
          guardian: primaryGuardian
            ? `${primaryGuardian.firstName} ${primaryGuardian.lastName}`
            : 'No guardian',
          athleteCount: family.athletes.length,
          status: family.isActive ? 'Active' : 'Inactive',
        };
      })
      .filter((row) => {
        const value = search.toLowerCase();
        return (
          row.familyName.toLowerCase().includes(value) ||
          row.organization.toLowerCase().includes(value) ||
          row.guardian.toLowerCase().includes(value)
        );
      });
  }, [families, search]);

  const columns: GridColDef[] = [
    { field: 'familyName', headerName: 'Family', flex: 1, minWidth: 160 },
    { field: 'organization', headerName: 'Organization', flex: 1.3, minWidth: 220 },
    { field: 'guardian', headerName: 'Primary Guardian', flex: 1, minWidth: 180 },
    { field: 'athleteCount', headerName: 'Athletes', width: 110 },
    {
      field: 'status',
      headerName: 'Status',
      width: 130,
      renderCell: (params) => (
        <Chip
          label={params.value}
          size="small"
          color={params.value === 'Active' ? 'success' : 'default'}
          variant="outlined"
        />
      ),
    },
  ];

  return (
    <>
      <PageHeader
        title="Families"
        subtitle="Manage family accounts, guardians, athletes, and organization relationships."
        action={
          <Button variant="contained" startIcon={<AddIcon />}>
            Register Family
          </Button>
        }
      />

      {loading && <CircularProgress />}
      {error && <Alert severity="error">{error}</Alert>}

      {!loading && !error && (
        <Box>
          <TextField
            fullWidth
            label="Search families"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            sx={{ mb: 2 }}
          />

          <Box sx={{ height: 520, width: '100%' }}>
            <DataGrid
              rows={rows}
              columns={columns}
              pageSizeOptions={[10, 25, 50]}
              initialState={{
                pagination: {
                  paginationModel: { pageSize: 10, page: 0 },
                },
              }}
              disableRowSelectionOnClick
              onRowClick={(params) => navigate(`/families/${params.id}`)}
              sx={{
                cursor: 'pointer',
                '& .MuiDataGrid-row:hover': {
                  backgroundColor: 'rgba(255,255,255,0.06)',
                },
              }}
            />
          </Box>
        </Box>
      )}
    </>
  );
}