import { useCallback, useEffect, useMemo, useState } from 'react'
import { Alert, Box, Button, Card, CardActionArea, CardContent, Chip, Container, Divider, Grid, MenuItem, Paper, Stack, TextField, Typography } from '@mui/material'
import { Link } from 'react-router'
import { apiRequest } from '../services/api.js'

const money = (value) => new Intl.NumberFormat('es-PA', { style: 'currency', currency: 'PAB' }).format(value ?? 0)

export default function SalesPage({ session }) {
  const [catalog, setCatalog] = useState({ tariffs: [], paymentMethods: [] }); const [cash, setCash] = useState(null); const [recent, setRecent] = useState([])
  const [cart, setCart] = useState([]); const [search, setSearch] = useState(''); const [paymentMethodId, setPaymentMethodId] = useState(''); const [error, setError] = useState(''); const [message, setMessage] = useState('')
  const load = useCallback(async () => {
    try {
      const [data, current, sales] = await Promise.all([apiRequest('/sales/catalog', session.accessToken), apiRequest('/sales/cash/current', session.accessToken), apiRequest('/sales/recent', session.accessToken)])
      setCatalog(data); setCash(current); setRecent(sales); if (!paymentMethodId && data.paymentMethods.length) setPaymentMethodId(data.paymentMethods[0].id); setError('')
    } catch (err) { setError(err.message) }
  }, [paymentMethodId, session.accessToken])
  useEffect(() => { load() }, [load])
  const filtered = catalog.tariffs.filter((x) => `${x.code} ${x.description}`.toLowerCase().includes(search.toLowerCase()))
  const total = useMemo(() => cart.reduce((sum, item) => sum + item.price * item.quantity, 0), [cart])
  function add(item) { setCart((current) => { const found = current.find((x) => x.id === item.id); return found ? current.map((x) => x.id === item.id ? { ...x, quantity: x.quantity + 1 } : x) : [...current, { ...item, quantity: 1 }] }) }
  function quantity(id, value) { const next = Math.max(0, Number(value)); setCart((current) => next === 0 ? current.filter((x) => x.id !== id) : current.map((x) => x.id === id ? { ...x, quantity: next } : x)) }
  async function charge() {
    setError(''); setMessage('')
    try {
      const sale = await apiRequest('/sales', session.accessToken, { method: 'POST', body: JSON.stringify({ lines: cart.map((x) => ({ tariffId: x.id, quantity: x.quantity })), payments: [{ paymentMethodId: Number(paymentMethodId), amount: total }] }) })
      setCart([]); setMessage(`Venta ${sale.invoiceNumber} registrada por ${money(sale.total)}.`); await load()
    } catch (err) { setError(err.message) }
  }

  return <Container maxWidth="xl" sx={{ py: 4 }}>
    <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 2, alignItems: 'flex-start', mb: 3, flexWrap: 'wrap' }}><Box><Typography variant="overline" color="secondary.main" fontWeight={800}>Punto de venta</Typography><Typography variant="h4">Nueva venta</Typography><Typography color="text.secondary">Productos y servicios postales disponibles.</Typography></Box><Chip label={cash?.isOpen ? `Caja abierta · ${cash.terminal}` : 'Caja cerrada'} color={cash?.isOpen ? 'success' : 'default'} /></Box>
    {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}{message && <Alert severity="success" sx={{ mb: 2 }}>{message}</Alert>}
    {!cash?.isOpen && <Alert severity="warning" sx={{ mb: 3 }} action={(session.permissions ?? []).includes('caja.access') ? <Button component={Link} to="/caja" color="inherit">Ir a caja</Button> : null}>Debes abrir una caja antes de registrar ventas.</Alert>}
    <Grid container spacing={3}><Grid size={{ xs: 12, lg: 8 }}><TextField fullWidth placeholder="Buscar por código o descripción" value={search} onChange={(e) => setSearch(e.target.value)} sx={{ mb: 2, bgcolor: 'background.paper' }} />
      <Grid container spacing={2}>{filtered.map((item) => <Grid key={item.id} size={{ xs: 12, sm: 6, md: 4 }}><Card elevation={0} sx={{ height: '100%', border: '1px solid', borderColor: 'divider', transition: '.2s', '&:hover': { transform: 'translateY(-3px)', boxShadow: 4, borderColor: 'secondary.main' } }}><CardActionArea onClick={() => add(item)} sx={{ height: '100%' }}><CardContent><Chip label={item.type} size="small" variant="outlined" /><Typography variant="h6" color="primary" sx={{ mt: 2 }}>{item.description}</Typography><Typography variant="caption" color="text.secondary">{item.code}</Typography><Typography variant="h5" sx={{ mt: 2 }}>{money(item.price)}</Typography></CardContent></CardActionArea></Card></Grid>)}</Grid>
      <Typography variant="h6" sx={{ mt: 5, mb: 2 }}>Ventas recientes</Typography><Paper elevation={0} sx={{ border: '1px solid', borderColor: 'divider' }}>{recent.length ? recent.map((sale) => <Box key={sale.id} sx={{ display: 'flex', justifyContent: 'space-between', p: 2, borderBottom: '1px solid', borderColor: 'divider' }}><Box><Typography fontWeight={700}>{sale.invoiceNumber}</Typography><Typography variant="caption" color="text.secondary">{new Date(sale.createdAtUtc).toLocaleString('es-PA')}</Typography></Box><Typography fontWeight={800}>{money(sale.total)}</Typography></Box>) : <Typography color="text.secondary" sx={{ p: 2 }}>Aún no hay ventas.</Typography>}</Paper>
    </Grid><Grid size={{ xs: 12, lg: 4 }}><Paper elevation={5} sx={{ p: 3, position: { lg: 'sticky' }, top: 88, borderRadius: 3 }}><Typography variant="h5" color="primary">Detalle de venta</Typography><Divider sx={{ my: 2 }} />{cart.length ? <Stack spacing={2}>{cart.map((item) => <Box key={item.id} sx={{ display: 'grid', gridTemplateColumns: '1fr 74px auto', alignItems: 'center', gap: 1 }}><Box><Typography fontWeight={700}>{item.description}</Typography><Typography variant="caption" color="text.secondary">{money(item.price)} c/u</Typography></Box><TextField type="number" size="small" value={item.quantity} inputProps={{ min: 0 }} onChange={(e) => quantity(item.id, e.target.value)} /><Typography fontWeight={800}>{money(item.price * item.quantity)}</Typography></Box>)}</Stack> : <Box sx={{ py: 5, textAlign: 'center' }}><Typography color="text.secondary">Selecciona productos o servicios para comenzar.</Typography></Box>}<Divider sx={{ my: 3 }} /><Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}><Typography variant="h6">Total</Typography><Typography variant="h4" color="primary">{money(total)}</Typography></Box><TextField select fullWidth label="Forma de pago" value={paymentMethodId} onChange={(e) => setPaymentMethodId(e.target.value)} sx={{ mb: 2 }}>{catalog.paymentMethods.map((item) => <MenuItem key={item.id} value={item.id}>{item.name}</MenuItem>)}</TextField><Button fullWidth size="large" variant="contained" disabled={!cash?.isOpen || !cart.length || !paymentMethodId} onClick={charge}>Cobrar {money(total)}</Button></Paper></Grid></Grid>
  </Container>
}
