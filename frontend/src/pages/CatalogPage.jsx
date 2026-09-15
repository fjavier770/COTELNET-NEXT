import { useCallback, useEffect, useState } from 'react'
import { Alert, Box, Button, Container, Dialog, DialogActions, DialogContent, DialogTitle, FormControlLabel, MenuItem, Paper, Switch, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Typography } from '@mui/material'
import { apiRequest } from '../services/api.js'

export default function CatalogPage({ kind, session }) {
  const isTerminal = kind === 'terminals'
  const empty = isTerminal ? { code: '', name: '', macAddress: '', estafetaId: '', active: true } : { codigo: '', nombre: '', activa: true }
  const [items, setItems] = useState([]); const [estafetas, setEstafetas] = useState([]); const [form, setForm] = useState(empty)
  const [editing, setEditing] = useState(null); const [open, setOpen] = useState(false); const [error, setError] = useState('')
  const title = isTerminal ? 'Terminales' : 'Estafetas'

  const load = useCallback(async () => {
    try {
      const data = await apiRequest(`/administration/${kind}`, session.accessToken); setItems(data)
      if (isTerminal) setEstafetas((await apiRequest('/administration/terminals/estafetas', session.accessToken)).filter((x) => x.activa))
      setError('')
    } catch (err) { setError(err.message) }
  }, [isTerminal, kind, session.accessToken])
  useEffect(() => { load() }, [load])
  function show(item) { setEditing(item?.id ?? null); setForm(item ? { ...item, macAddress: item.macAddress ?? '' } : empty); setOpen(true) }
  async function save(event) {
    event.preventDefault()
    const body = isTerminal ? { ...form, estafetaId: Number(form.estafetaId), macAddress: form.macAddress || null } : form
    try { await apiRequest(`/administration/${kind}${editing ? `/${editing}` : ''}`, session.accessToken, { method: editing ? 'PUT' : 'POST', body: JSON.stringify(body) }); setOpen(false); await load() } catch (err) { setError(err.message) }
  }

  return <Container maxWidth="xl" sx={{ py: 4 }}>
    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}><Box><Typography variant="h4">{title}</Typography><Typography color="text.secondary">{isTerminal ? 'Equipos autorizados y su ubicación operativa.' : 'Oficinas y puntos de atención.'}</Typography></Box><Button variant="contained" onClick={() => show(null)}>Nueva {isTerminal ? 'terminal' : 'estafeta'}</Button></Box>
    {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
    <TableContainer component={Paper}><Table><TableHead><TableRow><TableCell>Código</TableCell><TableCell>Nombre</TableCell>{isTerminal && <TableCell>Estafeta</TableCell>}{isTerminal && <TableCell>Dirección MAC</TableCell>}<TableCell>Estado</TableCell><TableCell /></TableRow></TableHead><TableBody>{items.map((item) => <TableRow key={item.id}><TableCell>{isTerminal ? item.code : item.codigo}</TableCell><TableCell>{isTerminal ? item.name : item.nombre}</TableCell>{isTerminal && <TableCell>{item.estafeta}</TableCell>}{isTerminal && <TableCell>{item.macAddress ?? 'No registrada'}</TableCell>}<TableCell>{(isTerminal ? item.active : item.activa) ? 'Activa' : 'Inactiva'}</TableCell><TableCell align="right"><Button onClick={() => show(item)}>Editar</Button></TableCell></TableRow>)}</TableBody></Table></TableContainer>
    <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="sm"><Box component="form" onSubmit={save}><DialogTitle>{editing ? `Editar ${isTerminal ? 'terminal' : 'estafeta'}` : `Nueva ${isTerminal ? 'terminal' : 'estafeta'}`}</DialogTitle><DialogContent sx={{ display: 'grid', gap: 2, pt: '12px !important' }}>
      <TextField label="Código" value={isTerminal ? form.code : form.codigo} required onChange={(e) => setForm({ ...form, [isTerminal ? 'code' : 'codigo']: e.target.value })} />
      <TextField label="Nombre" value={isTerminal ? form.name : form.nombre} required onChange={(e) => setForm({ ...form, [isTerminal ? 'name' : 'nombre']: e.target.value })} />
      {isTerminal && <TextField select label="Estafeta" value={form.estafetaId} required onChange={(e) => setForm({ ...form, estafetaId: e.target.value })}>{estafetas.map((item) => <MenuItem key={item.id} value={item.id}>{item.codigo} - {item.nombre}</MenuItem>)}</TextField>}
      {isTerminal && <TextField label="Dirección MAC (opcional)" value={form.macAddress} onChange={(e) => setForm({ ...form, macAddress: e.target.value })} />}
      {editing && <FormControlLabel control={<Switch checked={isTerminal ? form.active : form.activa} onChange={(e) => setForm({ ...form, [isTerminal ? 'active' : 'activa']: e.target.checked })} />} label="Registro activo" />}
    </DialogContent><DialogActions><Button onClick={() => setOpen(false)}>Cancelar</Button><Button type="submit" variant="contained">Guardar</Button></DialogActions></Box></Dialog>
  </Container>
}
