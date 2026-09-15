import { useCallback, useEffect, useState } from 'react'
import { Alert, Box, Button, Card, CardContent, Chip, Container, Dialog, DialogActions, DialogContent, DialogTitle, Grid, MenuItem, Paper, Stack, TextField, Typography } from '@mui/material'
import { apiRequest } from '../services/api.js'

const money = (value) => new Intl.NumberFormat('es-PA', { style: 'currency', currency: 'PAB' }).format(value ?? 0)

export default function CashPage({ session }) {
  const [cash, setCash] = useState(null); const [terminals, setTerminals] = useState([])
  const [terminalId, setTerminalId] = useState(''); const [openingAmount, setOpeningAmount] = useState('0.00'); const [declaredCash, setDeclaredCash] = useState('')
  const [closing, setClosing] = useState(false); const [error, setError] = useState(''); const [message, setMessage] = useState('')

  const load = useCallback(async () => {
    try {
      const [current, catalog] = await Promise.all([apiRequest('/cash/current', session.accessToken), apiRequest('/cash/catalog', session.accessToken)])
      setCash(current); setTerminals(catalog.terminals); if (!terminalId && catalog.terminals.length) setTerminalId(catalog.terminals[0].id); setError('')
    } catch (err) { setError(err.message) }
  }, [session.accessToken, terminalId])
  useEffect(() => { load() }, [load])

  async function openCash(event) {
    event.preventDefault(); setMessage('')
    try { const result = await apiRequest('/cash/open', session.accessToken, { method: 'POST', body: JSON.stringify({ terminalId: Number(terminalId), openingAmount: Number(openingAmount) }) }); setCash(result); setMessage('Caja abierta correctamente.') } catch (err) { setError(err.message) }
  }
  async function closeCash(event) {
    event.preventDefault()
    try { const result = await apiRequest('/cash/close', session.accessToken, { method: 'POST', body: JSON.stringify({ declaredCash: Number(declaredCash) }) }); setCash(result); setClosing(false); setMessage(`Caja cerrada. Diferencia: ${money(result.difference)}`) } catch (err) { setError(err.message) }
  }

  return <Container maxWidth="xl" sx={{ py: 4 }}>
    <Box sx={{ mb: 4 }}><Typography variant="overline" color="secondary.main" fontWeight={800}>Operación diaria</Typography><Typography variant="h4">Control de caja</Typography><Typography color="text.secondary">Apertura, recaudación y cierre de la terminal.</Typography></Box>
    {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}{message && <Alert severity="success" sx={{ mb: 2 }}>{message}</Alert>}
    {!cash || !cash.isOpen ? <Paper component="form" onSubmit={openCash} elevation={0} sx={{ maxWidth: 620, p: { xs: 3, md: 5 }, border: '1px solid', borderColor: 'divider', background: 'linear-gradient(145deg, #fff 55%, #eef5fb)' }}>
      <Typography variant="h5" color="primary" gutterBottom>Abrir una nueva caja</Typography><Typography color="text.secondary" sx={{ mb: 3 }}>Selecciona la terminal e indica el fondo inicial disponible.</Typography>
      {terminals.length ? <Stack spacing={3}><TextField select label="Terminal" value={terminalId} required onChange={(e) => setTerminalId(e.target.value)}>{terminals.map((item) => <MenuItem key={item.id} value={item.id}>{item.code} — {item.name} · {item.estafeta}</MenuItem>)}</TextField><TextField label="Fondo inicial" type="number" value={openingAmount} inputProps={{ min: 0, step: '0.01' }} onChange={(e) => setOpeningAmount(e.target.value)} required /><Button type="submit" variant="contained" size="large">Abrir caja</Button></Stack> : <Alert severity="warning">No hay terminales activas asignadas a tu estafeta.</Alert>}
    </Paper> : <>
      <Paper elevation={0} sx={{ p: 3, mb: 3, color: 'white', background: 'linear-gradient(120deg, #002f59, #075f96)', borderRadius: 3 }}><Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: 2, flexWrap: 'wrap' }}><Box><Chip label="Caja abierta" color="success" size="small" sx={{ mb: 1 }} /><Typography variant="h5">{cash.terminal}</Typography><Typography sx={{ opacity: .8 }}>{cash.estafeta} · Apertura {new Date(cash.openedAtUtc).toLocaleString('es-PA')}</Typography></Box><Button variant="contained" color="secondary" onClick={() => { setDeclaredCash(cash.expectedCash.toFixed(2)); setClosing(true) }}>Cerrar caja</Button></Box></Paper>
      <Grid container spacing={2} sx={{ mb: 3 }}>{[['Fondo inicial', cash.openingAmount], ['Ventas acumuladas', cash.salesTotal], ['Ventas en efectivo', cash.cashSales], ['Efectivo esperado', cash.expectedCash]].map(([label, value]) => <Grid key={label} size={{ xs: 12, sm: 6, lg: 3 }}><Card elevation={0} sx={{ border: '1px solid', borderColor: 'divider', height: '100%' }}><CardContent><Typography variant="body2" color="text.secondary">{label}</Typography><Typography variant="h4" color="primary" sx={{ mt: 1 }}>{money(value)}</Typography></CardContent></Card></Grid>)}</Grid>
      <Paper elevation={0} sx={{ p: 3, border: '1px solid', borderColor: 'divider' }}><Typography variant="h6" gutterBottom>Recaudación por forma de pago</Typography>{cash.payments.length ? cash.payments.map((item) => <Box key={item.paymentMethod} sx={{ display: 'flex', justifyContent: 'space-between', py: 1.5, borderBottom: '1px solid', borderColor: 'divider' }}><Typography>{item.paymentMethod}</Typography><Typography fontWeight={800}>{money(item.amount)}</Typography></Box>) : <Typography color="text.secondary">Todavía no hay ventas registradas en esta caja.</Typography>}</Paper>
    </>}
    <Dialog open={closing} onClose={() => setClosing(false)} fullWidth maxWidth="xs"><Box component="form" onSubmit={closeCash}><DialogTitle>Cerrar caja</DialogTitle><DialogContent sx={{ pt: '12px !important' }}><Alert severity="info" sx={{ mb: 2 }}>El efectivo esperado es {money(cash?.expectedCash)}.</Alert><TextField fullWidth label="Efectivo contado" type="number" value={declaredCash} inputProps={{ min: 0, step: '0.01' }} onChange={(e) => setDeclaredCash(e.target.value)} required /></DialogContent><DialogActions><Button onClick={() => setClosing(false)}>Cancelar</Button><Button type="submit" variant="contained">Confirmar cierre</Button></DialogActions></Box></Dialog>
  </Container>
}
