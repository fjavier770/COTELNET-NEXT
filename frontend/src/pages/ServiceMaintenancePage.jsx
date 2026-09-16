import { useCallback, useEffect, useState } from 'react'
import { Alert, Box, Button, Card, CardContent, CircularProgress, Container, Dialog, DialogActions, DialogContent, DialogTitle, FormControlLabel, Grid, MenuItem, Paper, Stack, Switch, Tab, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Tabs, TextField, Typography } from '@mui/material'
import { apiRequest } from '../services/api.js'

const root = '/administration/service-catalog'
const money = (value) => new Intl.NumberFormat('es-PA', { style: 'currency', currency: 'PAB' }).format(value ?? 0)
const blankForms = [
  { code: '', name: '', s10Prefix: '', active: true },
  { code: '', name: '', zone: '', isDomestic: false, active: true },
  { postalServiceId: '', destinationZone: '', minimumWeightGrams: 0, maximumWeightGrams: 500, price: 0, active: true },
  { code: '', name: '', price: 0, active: true },
]
const endpoints = ['postal-services', 'destinations', 'weight-tariffs', 'supplementary-services']

export default function ServiceMaintenancePage({ session }) {
  const [tab, setTab] = useState(0); const [services, setServices] = useState([]); const [destinations, setDestinations] = useState([]); const [tariffs, setTariffs] = useState([]); const [supplementary, setSupplementary] = useState([])
  const [open, setOpen] = useState(false); const [editing, setEditing] = useState(null); const [form, setForm] = useState(blankForms[0]); const [error, setError] = useState(''); const [message, setMessage] = useState('')
  const [legacyConfigured, setLegacyConfigured] = useState(false); const [importing, setImporting] = useState(false); const [importResult, setImportResult] = useState(null)
  const lists = [services, destinations, tariffs, supplementary]

  const load = useCallback(async () => {
    try {
      const [data, status] = await Promise.all([Promise.all(endpoints.map((endpoint) => apiRequest(`${root}/${endpoint}`, session.accessToken))), apiRequest(`${root}/legacy-import/status`, session.accessToken)])
      setServices(data[0]); setDestinations(data[1]); setTariffs(data[2]); setSupplementary(data[3]); setLegacyConfigured(status.configured); setError('')
    } catch (err) { setError(err.message) }
  }, [session.accessToken])
  useEffect(() => { load() }, [load])

  function show(item = null) { setEditing(item?.id ?? null); setForm(item ? { ...item } : { ...blankForms[tab] }); setOpen(true); setError(''); setMessage('') }
  async function save(event) {
    event.preventDefault()
    try {
      const body = tab === 2 ? { ...form, postalServiceId: Number(form.postalServiceId), minimumWeightGrams: Number(form.minimumWeightGrams), maximumWeightGrams: Number(form.maximumWeightGrams), price: Number(form.price) } : tab === 3 ? { ...form, price: Number(form.price) } : form
      await apiRequest(`${root}/${endpoints[tab]}${editing ? `/${editing}` : ''}`, session.accessToken, { method: editing ? 'PUT' : 'POST', body: JSON.stringify(body) })
      setOpen(false); setMessage('Los cambios fueron guardados correctamente.'); await load()
    } catch (err) { setError(err.message) }
  }

  async function importLegacyCatalog() {
    if (!window.confirm('Se reemplazarán las tarifas y catálogos operativos actuales con los datos de COTELNET. Las ventas y envíos existentes se conservarán. ¿Deseas continuar?')) return
    setImporting(true); setError(''); setMessage(''); setImportResult(null)
    try {
      const result = await apiRequest(`${root}/legacy-import`, session.accessToken, { method: 'POST' })
      setImportResult(result); setMessage('Los catálogos reales de COTELNET fueron importados correctamente.'); await load()
    } catch (err) { setError(err.message) } finally { setImporting(false) }
  }

  const activeCounts = [services.filter((x) => x.active).length, destinations.filter((x) => x.active).length, tariffs.filter((x) => x.active).length, supplementary.filter((x) => x.active).length]
  const titles = ['Servicios postales', 'Destinos y grupos', 'Tarifas por peso', 'Servicios suplementarios']

  return <Container maxWidth="xl" sx={{ py: 4 }}>
    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: 2, flexWrap: 'wrap', mb: 3 }}><Box><Typography variant="overline" color="secondary.main" fontWeight={800}>Configuración postal</Typography><Typography variant="h4">Mantenimiento de servicios</Typography><Typography color="text.secondary">Catálogos utilizados para cotizar y admitir envíos.</Typography></Box><Stack direction="row" spacing={1}><Button variant="outlined" disabled={!legacyConfigured || importing} onClick={importLegacyCatalog}>{importing ? <><CircularProgress size={18} sx={{ mr: 1 }} />Importando</> : 'Importar desde COTELNET'}</Button><Button variant="contained" onClick={() => show()}>Nuevo registro</Button></Stack></Box>
    {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}{message && <Alert severity="success" sx={{ mb: 2 }}>{message}</Alert>}
    {!legacyConfigured && <Alert severity="info" sx={{ mb: 2 }}>Para habilitar la importación, configura <strong>LEGACY_DB_CONNECTION_STRING</strong> en el archivo .env y reinicia la API.</Alert>}
    {importResult && <Alert severity="success" sx={{ mb: 2 }}>Importados: {importResult.postalServices} servicios, {importResult.destinations} destinos, {importResult.weightBands} rangos de peso, {importResult.supplementaryServices} suplementarios y {importResult.supplementaryOptions} asociaciones.</Alert>}
    <Grid container spacing={2} sx={{ mb: 3 }}>{titles.map((title, index) => <Grid key={title} size={{ xs: 6, lg: 3 }}><Card elevation={0} sx={{ border: '1px solid', borderColor: tab === index ? 'secondary.main' : 'divider', cursor: 'pointer' }} onClick={() => setTab(index)}><CardContent><Typography variant="body2" color="text.secondary">{title}</Typography><Typography variant="h4" color="primary">{activeCounts[index]}</Typography><Typography variant="caption">registros activos</Typography></CardContent></Card></Grid>)}</Grid>
    <Paper elevation={0} sx={{ border: '1px solid', borderColor: 'divider' }}><Tabs value={tab} onChange={(_, value) => setTab(value)} variant="scrollable" scrollButtons="auto" sx={{ px: 2, borderBottom: '1px solid', borderColor: 'divider' }}>{titles.map((title) => <Tab key={title} label={title} />)}</Tabs>
      <TableContainer><Table><TableHead><TableRow>{tab !== 2 && <TableCell>Código</TableCell>}<TableCell>{tab === 2 ? 'Servicio' : 'Nombre'}</TableCell>{tab === 0 && <TableCell>Prefijo S10</TableCell>}{tab === 1 && <TableCell>Grupo</TableCell>}{tab === 1 && <TableCell>Tipo</TableCell>}{tab === 2 && <TableCell>Grupo</TableCell>}{tab === 2 && <TableCell>Rango</TableCell>}{(tab === 2 || tab === 3) && <TableCell>Precio</TableCell>}<TableCell>Estado</TableCell><TableCell /></TableRow></TableHead><TableBody>
        {lists[tab].map((item) => <TableRow key={item.id}>{tab !== 2 && <TableCell>{item.code}</TableCell>}<TableCell>{tab === 2 ? item.postalService : item.name}</TableCell>{tab === 0 && <TableCell>{item.s10Prefix}</TableCell>}{tab === 1 && <TableCell>{item.zone}</TableCell>}{tab === 1 && <TableCell>{item.isDomestic ? 'Nacional' : 'Internacional'}</TableCell>}{tab === 2 && <TableCell>{item.destinationZone}</TableCell>}{tab === 2 && <TableCell>{item.minimumWeightGrams + 1}–{item.maximumWeightGrams} g</TableCell>}{(tab === 2 || tab === 3) && <TableCell>{money(item.price)}</TableCell>}<TableCell>{item.active ? 'Activo' : 'Inactivo'}</TableCell><TableCell align="right"><Button onClick={() => show(item)}>Editar</Button></TableCell></TableRow>)}
      </TableBody></Table></TableContainer>
    </Paper>
    <Dialog open={open} onClose={() => setOpen(false)} fullWidth maxWidth="sm"><Box component="form" onSubmit={save}><DialogTitle>{editing ? 'Editar' : 'Crear'} {titles[tab].toLowerCase()}</DialogTitle><DialogContent sx={{ display: 'grid', gap: 2, pt: '12px !important' }}>
      {(tab === 0 || tab === 1 || tab === 3) && <TextField label="Código" value={form.code ?? ''} required onChange={(e) => setForm({ ...form, code: e.target.value })} />}
      {(tab === 0 || tab === 1 || tab === 3) && <TextField label="Nombre" value={form.name ?? ''} required onChange={(e) => setForm({ ...form, name: e.target.value })} />}
      {tab === 0 && <TextField label="Prefijo S10 UPU" value={form.s10Prefix ?? ''} inputProps={{ maxLength: 2 }} helperText="Puede provenir del tipo de servicio o del suplementario, según la configuración de COTELNET." onChange={(e) => setForm({ ...form, s10Prefix: e.target.value.toUpperCase().replace(/[^A-Z]/g, '').slice(0, 2) })} />}
      {tab === 1 && <TextField label="Grupo tarifario" value={form.zone ?? ''} helperText="Grupo de países o provincias utilizado por la tarifa." required onChange={(e) => setForm({ ...form, zone: e.target.value })} />}
      {tab === 1 && <FormControlLabel control={<Switch checked={Boolean(form.isDomestic)} onChange={(e) => setForm({ ...form, isDomestic: e.target.checked })} />} label="Destino nacional" />}
      {tab === 2 && <TextField select label="Servicio postal" value={form.postalServiceId ?? ''} required onChange={(e) => setForm({ ...form, postalServiceId: e.target.value })}>{services.map((item) => <MenuItem key={item.id} value={item.id}>{item.code} — {item.name}</MenuItem>)}</TextField>}
      {tab === 2 && <TextField select label="Grupo tarifario" value={form.destinationZone ?? ''} required onChange={(e) => setForm({ ...form, destinationZone: e.target.value })}>{[...new Set(destinations.map((x) => x.zone))].map((zone) => <MenuItem key={zone} value={zone}>{zone}</MenuItem>)}</TextField>}
      {tab === 2 && <Grid container spacing={2}><Grid size={6}><TextField fullWidth type="number" label="Desde (g)" value={form.minimumWeightGrams ?? 0} inputProps={{ min: 0 }} onChange={(e) => setForm({ ...form, minimumWeightGrams: e.target.value })} /></Grid><Grid size={6}><TextField fullWidth type="number" label="Hasta (g)" value={form.maximumWeightGrams ?? 0} inputProps={{ min: 1 }} onChange={(e) => setForm({ ...form, maximumWeightGrams: e.target.value })} /></Grid></Grid>}
      {(tab === 2 || tab === 3) && <TextField type="number" label="Precio" value={form.price ?? 0} inputProps={{ min: 0, step: 0.01 }} required onChange={(e) => setForm({ ...form, price: e.target.value })} />}
      <FormControlLabel control={<Switch checked={Boolean(form.active)} onChange={(e) => setForm({ ...form, active: e.target.checked })} />} label="Registro activo" />
    </DialogContent><DialogActions><Button onClick={() => setOpen(false)}>Cancelar</Button><Button type="submit" variant="contained">Guardar</Button></DialogActions></Box></Dialog>
  </Container>
}
