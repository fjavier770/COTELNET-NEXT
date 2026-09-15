import { useCallback, useEffect, useState } from 'react'
import { Alert, Box, Button, Container, Dialog, DialogActions, DialogContent, DialogTitle, FormControlLabel, MenuItem, Paper, Switch, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Typography } from '@mui/material'
import { apiRequest } from '../services/api.js'

const emptyForm = { username: '', fullName: '', password: '', roleId: '', estafetaId: '', active: true }

export default function UsersPage({ session }) {
  const [users, setUsers] = useState([])
  const [roles, setRoles] = useState([])
  const [estafetas, setEstafetas] = useState([])
  const [form, setForm] = useState(emptyForm)
  const [editing, setEditing] = useState(null)
  const [open, setOpen] = useState(false)
  const [error, setError] = useState('')

  const load = useCallback(async () => {
    try {
      const [userData, roleData, estafetaData] = await Promise.all([
        apiRequest('/administration/users', session.accessToken),
        apiRequest('/administration/users/roles', session.accessToken),
        apiRequest('/administration/users/estafetas', session.accessToken),
      ])
      setUsers(userData); setRoles(roleData.filter((x) => x.active)); setEstafetas(estafetaData.filter((x) => x.activa)); setError('')
    } catch (err) { setError(err.message) }
  }, [session.accessToken])

  useEffect(() => { load() }, [load])

  function showCreate() { setEditing(null); setForm(emptyForm); setOpen(true) }
  function showEdit(user) {
    setEditing(user.id)
    setForm({ username: user.username, fullName: user.fullName, password: '', roleId: user.roleId, estafetaId: user.estafetaId ?? '', active: user.active })
    setOpen(true)
  }

  async function save(event) {
    event.preventDefault(); setError('')
    const body = editing
      ? { fullName: form.fullName, roleId: Number(form.roleId), estafetaId: form.estafetaId === '' ? null : Number(form.estafetaId), active: form.active }
      : { username: form.username, fullName: form.fullName, password: form.password, roleId: Number(form.roleId), estafetaId: form.estafetaId === '' ? null : Number(form.estafetaId) }
    try {
      await apiRequest(`/administration/users${editing ? `/${editing}` : ''}`, session.accessToken, { method: editing ? 'PUT' : 'POST', body: JSON.stringify(body) })
      setOpen(false); await load()
    } catch (err) { setError(err.message) }
  }

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}><Box><Typography variant="h4">Usuarios</Typography><Typography color="text.secondary">Acceso, rol y estafeta asignada.</Typography></Box><Button variant="contained" onClick={showCreate}>Nuevo usuario</Button></Box>
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      <TableContainer component={Paper}><Table><TableHead><TableRow><TableCell>Usuario</TableCell><TableCell>Nombre</TableCell><TableCell>Rol</TableCell><TableCell>Estafeta</TableCell><TableCell>Estado</TableCell><TableCell /></TableRow></TableHead><TableBody>
        {users.map((user) => <TableRow key={user.id}><TableCell>{user.username}</TableCell><TableCell>{user.fullName}</TableCell><TableCell>{user.role}</TableCell><TableCell>{user.estafeta ?? 'Sin asignar'}</TableCell><TableCell>{user.active ? 'Activo' : 'Inactivo'}</TableCell><TableCell align="right"><Button onClick={() => showEdit(user)}>Editar</Button></TableCell></TableRow>)}
      </TableBody></Table></TableContainer>
      <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="sm"><Box component="form" onSubmit={save}><DialogTitle>{editing ? 'Editar usuario' : 'Nuevo usuario'}</DialogTitle><DialogContent sx={{ display: 'grid', gap: 2, pt: '12px !important' }}>
        <TextField label="Usuario" value={form.username} disabled={Boolean(editing)} required onChange={(e) => setForm({ ...form, username: e.target.value })} />
        <TextField label="Nombre completo" value={form.fullName} required onChange={(e) => setForm({ ...form, fullName: e.target.value })} />
        {!editing && <TextField label="Contraseña inicial" type="password" value={form.password} required inputProps={{ minLength: 8 }} onChange={(e) => setForm({ ...form, password: e.target.value })} />}
        <TextField select label="Rol" value={form.roleId} required onChange={(e) => setForm({ ...form, roleId: e.target.value })}>{roles.map((role) => <MenuItem key={role.id} value={role.id}>{role.name}</MenuItem>)}</TextField>
        <TextField select label="Estafeta" value={form.estafetaId} onChange={(e) => setForm({ ...form, estafetaId: e.target.value })}><MenuItem value="">Sin asignar</MenuItem>{estafetas.map((item) => <MenuItem key={item.id} value={item.id}>{item.codigo} - {item.nombre}</MenuItem>)}</TextField>
        {editing && <FormControlLabel control={<Switch checked={form.active} onChange={(e) => setForm({ ...form, active: e.target.checked })} />} label="Usuario activo" />}
      </DialogContent><DialogActions><Button onClick={() => setOpen(false)}>Cancelar</Button><Button type="submit" variant="contained">Guardar</Button></DialogActions></Box></Dialog>
    </Container>
  )
}
