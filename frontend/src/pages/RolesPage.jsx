import { useCallback, useEffect, useState } from 'react'
import { Alert, Box, Button, Checkbox, Container, Dialog, DialogActions, DialogContent, DialogTitle, FormControlLabel, FormGroup, Paper, Switch, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Typography } from '@mui/material'
import { apiRequest } from '../services/api.js'

const emptyForm = { name: '', description: '', active: true, permissionIds: [] }

export default function RolesPage({ session }) {
  const [roles, setRoles] = useState([]); const [permissions, setPermissions] = useState([])
  const [form, setForm] = useState(emptyForm); const [editing, setEditing] = useState(null); const [open, setOpen] = useState(false); const [error, setError] = useState('')
  const load = useCallback(async () => { try { const [r, p] = await Promise.all([apiRequest('/administration/roles', session.accessToken), apiRequest('/administration/permissions', session.accessToken)]); setRoles(r); setPermissions(p); setError('') } catch (err) { setError(err.message) } }, [session.accessToken])
  useEffect(() => { load() }, [load])
  function show(role) { setEditing(role?.id ?? null); setForm(role ? { name: role.name, description: role.description, active: role.active, permissionIds: role.permissionIds } : emptyForm); setOpen(true) }
  function toggle(id) { setForm({ ...form, permissionIds: form.permissionIds.includes(id) ? form.permissionIds.filter((x) => x !== id) : [...form.permissionIds, id] }) }
  async function save(event) { event.preventDefault(); try { await apiRequest(`/administration/roles${editing ? `/${editing}` : ''}`, session.accessToken, { method: editing ? 'PUT' : 'POST', body: JSON.stringify(form) }); setOpen(false); await load() } catch (err) { setError(err.message) } }
  const modules = [...new Set(permissions.map((x) => x.module))]
  return <Container maxWidth="xl" sx={{ py: 4 }}>
    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}><Box><Typography variant="h4">Roles y permisos</Typography><Typography color="text.secondary">Control de acceso por función.</Typography></Box><Button variant="contained" onClick={() => show(null)}>Nuevo rol</Button></Box>
    {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
    <TableContainer component={Paper}><Table><TableHead><TableRow><TableCell>Rol</TableCell><TableCell>Descripción</TableCell><TableCell>Permisos</TableCell><TableCell>Estado</TableCell><TableCell /></TableRow></TableHead><TableBody>{roles.map((role) => <TableRow key={role.id}><TableCell>{role.name}</TableCell><TableCell>{role.description}</TableCell><TableCell>{role.permissions.length}</TableCell><TableCell>{role.active ? 'Activo' : 'Inactivo'}</TableCell><TableCell align="right"><Button onClick={() => show(role)}>Editar</Button></TableCell></TableRow>)}</TableBody></Table></TableContainer>
    <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="md"><Box component="form" onSubmit={save}><DialogTitle>{editing ? 'Editar rol' : 'Nuevo rol'}</DialogTitle><DialogContent sx={{ pt: '12px !important' }}><Box sx={{ display: 'grid', gap: 2 }}><TextField label="Nombre" value={form.name} required onChange={(e) => setForm({ ...form, name: e.target.value })} /><TextField label="Descripción" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />{editing && <FormControlLabel control={<Switch checked={form.active} onChange={(e) => setForm({ ...form, active: e.target.checked })} />} label="Rol activo" />}</Box>
      {modules.map((module) => <Box key={module} sx={{ mt: 3 }}><Typography fontWeight={700}>{module}</Typography><FormGroup row>{permissions.filter((x) => x.module === module).map((permission) => <FormControlLabel key={permission.id} control={<Checkbox checked={form.permissionIds.includes(permission.id)} onChange={() => toggle(permission.id)} />} label={permission.name} />)}</FormGroup></Box>)}
    </DialogContent><DialogActions><Button onClick={() => setOpen(false)}>Cancelar</Button><Button type="submit" variant="contained">Guardar</Button></DialogActions></Box></Dialog>
  </Container>
}
