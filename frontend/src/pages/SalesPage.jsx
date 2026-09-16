import { useCallback, useEffect, useState } from 'react'
import { Alert, Box, Button, Checkbox, Chip, Container, Divider, FormControlLabel, FormGroup, Grid, MenuItem, Paper, Stack, Step, StepLabel, Stepper, TextField, Typography } from '@mui/material'
import { Link } from 'react-router'
import { apiRequest } from '../services/api.js'

const money = (value) => new Intl.NumberFormat('es-PA', { style: 'currency', currency: 'PAB' }).format(value ?? 0)
const initialForm = { senderName: '', senderDocument: '', senderPhone: '', senderEmail: '', senderAddress: '', recipientName: '', recipientPhone: '', recipientAddress: '', postalServiceId: '', destinationId: '', weightKg: '', supplementaryServiceIds: [], paymentMethodId: '' }
const steps = ['Remitente', 'Destinatario', 'Envío', 'Suplementarios', 'Cobro']
const safe = (value) => String(value ?? '').replace(/[&<>"']/g, (character) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[character])

function printDocument(title, body, pageSize = 'auto') {
  const popup = window.open('', '_blank', 'width=800,height=900')
  if (!popup) return
  popup.document.write(`<!doctype html><html><head><title>${safe(title)}</title><style>@page{size:${pageSize};margin:10mm}body{font-family:Arial,sans-serif;color:#10233a;margin:0}h1{color:#003b70;margin:0}h2{margin:8px 0}.muted{color:#607080}.row{display:flex;justify-content:space-between;gap:20px}.box{border:1px solid #ccd5df;border-radius:8px;padding:14px;margin:12px 0}table{width:100%;border-collapse:collapse}th,td{text-align:left;padding:8px;border-bottom:1px solid #ddd}.total{font-size:24px;font-weight:700;color:#003b70}.qr svg{width:160px;height:160px}small{font-size:11px}</style></head><body>${body}<script>window.onload=()=>window.print()</script></body></html>`)
  popup.document.close()
}

function printInvoice(sale) {
  const shipment = sale.shipment
  printDocument(sale.invoiceNumber, `<div class="row"><div><h1>COTELNET</h1><div class="muted">Correos Panamá</div></div><div><h2>Factura</h2><strong>${safe(sale.invoiceNumber)}</strong><br><small>${new Date(sale.createdAtUtc).toLocaleString('es-PA')}</small></div></div><div class="box"><strong>Remitente:</strong> ${safe(shipment.senderName)} · ${safe(shipment.senderDocument)}<br>${safe(shipment.senderAddress)}<br>${safe(shipment.senderPhone)} ${safe(shipment.senderEmail)}</div><div class="box"><strong>Destinatario:</strong> ${safe(shipment.recipientName)}<br>${safe(shipment.recipientAddress)} · ${safe(shipment.destination)}<br>${safe(shipment.recipientPhone)}</div><table><thead><tr><th>Concepto</th><th>Cant.</th><th>Precio</th><th>Total</th></tr></thead><tbody>${sale.lines.map((line) => `<tr><td>${safe(line.description)}</td><td>${line.quantity}</td><td>${money(line.unitPrice)}</td><td>${money(line.total)}</td></tr>`).join('')}</tbody></table><div class="row box"><strong>Total pagado</strong><span class="total">${money(sale.total)}</span></div><small>Seguimiento: ${safe(shipment.trackingNumber)} · Pago: ${safe(sale.payments[0]?.paymentMethod)}</small>`)
}

function printLabel(sale) {
  const shipment = sale.shipment
  printDocument(shipment.trackingNumber, `<h1>COTELNET</h1><div class="muted">Etiqueta de envío</div><div class="box"><strong>DE:</strong><h2>${safe(shipment.senderName)}</h2>${safe(shipment.senderAddress)}<br>${safe(shipment.senderPhone)}</div><div class="box"><strong>PARA:</strong><h2>${safe(shipment.recipientName)}</h2>${safe(shipment.recipientAddress)}<br><strong>${safe(shipment.destination)}</strong><br>${safe(shipment.recipientPhone)}</div><div class="row"><div><strong>${safe(shipment.postalService)}</strong><br>Peso: ${shipment.weightGrams} g<br>${shipment.supplementaryServices.map((x) => safe(x.name)).join(' · ')}</div><div class="qr">${shipment.barcodeSvg}</div></div><div style="text-align:center;font-size:20px;font-weight:700;letter-spacing:2px">${safe(shipment.trackingNumber)}</div>`, '4in 6in')
}

export default function SalesPage({ session }) {
  const [catalog, setCatalog] = useState({ paymentMethods: [], postalServices: [], destinations: [], supplementaryServices: [] })
  const [cash, setCash] = useState(null); const [recent, setRecent] = useState([]); const [form, setForm] = useState(initialForm); const [step, setStep] = useState(0); const [quote, setQuote] = useState(null); const [completed, setCompleted] = useState(null); const [error, setError] = useState('')
  const load = useCallback(async () => {
    try {
      const [data, current, sales] = await Promise.all([apiRequest('/sales/catalog', session.accessToken), apiRequest('/sales/cash/current', session.accessToken), apiRequest('/sales/recent', session.accessToken)])
      setCatalog(data); setCash(current); setRecent(sales); setForm((currentForm) => ({ ...currentForm, paymentMethodId: currentForm.paymentMethodId || data.paymentMethods[0]?.id || '' })); setError('')
    } catch (err) { setError(err.message) }
  }, [session.accessToken])
  useEffect(() => { load() }, [load])

  function validateCurrentStep() {
    if (step === 0 && (!form.senderName.trim() || !form.senderAddress.trim())) return 'Completa el nombre y la dirección del remitente.'
    if (step === 1 && (!form.recipientName.trim() || !form.recipientAddress.trim())) return 'Completa el nombre y la dirección del destinatario.'
    if (step === 2 && (!form.postalServiceId || !form.destinationId || Number(form.weightKg) <= 0)) return 'Selecciona el servicio, destino e indica un peso válido.'
    return ''
  }

  async function calculateQuote() {
    const result = await apiRequest('/sales/quote', session.accessToken, { method: 'POST', body: JSON.stringify({ postalServiceId: Number(form.postalServiceId), destinationId: Number(form.destinationId), weightGrams: Math.round(Number(form.weightKg) * 1000), supplementaryServiceIds: form.supplementaryServiceIds }) })
    setQuote(result); return result
  }

  async function next() {
    const validation = validateCurrentStep(); if (validation) { setError(validation); return }
    setError('')
    try { if (step === 2 || step === 3) await calculateQuote(); setStep((value) => Math.min(4, value + 1)) } catch (err) { setError(err.message) }
  }

  function toggleSupplementary(id) {
    setForm({ ...form, supplementaryServiceIds: form.supplementaryServiceIds.includes(id) ? form.supplementaryServiceIds.filter((x) => x !== id) : [...form.supplementaryServiceIds, id] })
  }

  async function finish() {
    try {
      const result = await apiRequest('/sales/shipments', session.accessToken, { method: 'POST', body: JSON.stringify({ ...form, postalServiceId: Number(form.postalServiceId), destinationId: Number(form.destinationId), weightGrams: Math.round(Number(form.weightKg) * 1000), paymentMethodId: Number(form.paymentMethodId) }) })
      setCompleted(result); await load()
    } catch (err) { setError(err.message) }
  }

  function reset() { setForm({ ...initialForm, paymentMethodId: catalog.paymentMethods[0]?.id || '' }); setStep(0); setQuote(null); setCompleted(null); setError('') }

  if (completed) return <Container maxWidth="md" sx={{ py: 5 }}><Paper elevation={0} sx={{ p: { xs: 3, md: 6 }, textAlign: 'center', border: '1px solid', borderColor: 'divider', background: 'linear-gradient(145deg,#fff,#eef7f2)' }}><Chip color="success" label="Envío registrado" /><Typography variant="h3" color="primary" sx={{ mt: 2 }}>{completed.shipment.trackingNumber}</Typography><Typography color="text.secondary" sx={{ mt: 1 }}>Factura {completed.invoiceNumber} · Total {money(completed.total)}</Typography><Grid container spacing={2} sx={{ mt: 4 }}><Grid size={{ xs: 12, sm: 6 }}><Button fullWidth variant="contained" size="large" onClick={() => printLabel(completed)}>Imprimir etiqueta</Button></Grid><Grid size={{ xs: 12, sm: 6 }}><Button fullWidth variant="outlined" size="large" onClick={() => printInvoice(completed)}>Imprimir factura</Button></Grid></Grid><Button sx={{ mt: 3 }} onClick={reset}>Registrar otro envío</Button></Paper></Container>

  return <Container maxWidth="xl" sx={{ py: 4 }}>
    <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 2, mb: 3, flexWrap: 'wrap' }}><Box><Typography variant="overline" color="secondary.main" fontWeight={800}>Admisión postal</Typography><Typography variant="h4">Nuevo envío</Typography><Typography color="text.secondary">Registro, cálculo de tarifa y cobro en un solo flujo.</Typography></Box><Chip label={cash?.isOpen ? `Caja abierta · ${cash.terminal}` : 'Caja cerrada'} color={cash?.isOpen ? 'success' : 'default'} /></Box>
    {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}{!cash?.isOpen && <Alert severity="warning" sx={{ mb: 3 }} action={(session.permissions ?? []).includes('caja.access') ? <Button component={Link} to="/caja" color="inherit">Ir a caja</Button> : null}>Debes abrir una caja antes de registrar envíos.</Alert>}
    <Paper elevation={0} sx={{ p: { xs: 2, md: 4 }, border: '1px solid', borderColor: 'divider', borderRadius: 3 }}><Stepper activeStep={step} alternativeLabel sx={{ mb: 5 }}>{steps.map((label) => <Step key={label}><StepLabel>{label}</StepLabel></Step>)}</Stepper>
      {step === 0 && <Grid container spacing={2}><Grid size={{ xs: 12, md: 7 }}><TextField fullWidth label="Nombre completo del remitente" value={form.senderName} onChange={(e) => setForm({ ...form, senderName: e.target.value })} required /></Grid><Grid size={{ xs: 12, md: 5 }}><TextField fullWidth label="Cédula o identificación" value={form.senderDocument} onChange={(e) => setForm({ ...form, senderDocument: e.target.value })} /></Grid><Grid size={{ xs: 12, md: 6 }}><TextField fullWidth label="Teléfono" value={form.senderPhone} onChange={(e) => setForm({ ...form, senderPhone: e.target.value })} /></Grid><Grid size={{ xs: 12, md: 6 }}><TextField fullWidth type="email" label="Correo electrónico" value={form.senderEmail} onChange={(e) => setForm({ ...form, senderEmail: e.target.value })} /></Grid><Grid size={12}><TextField fullWidth multiline minRows={2} label="Dirección del remitente" value={form.senderAddress} onChange={(e) => setForm({ ...form, senderAddress: e.target.value })} required /></Grid></Grid>}
      {step === 1 && <Grid container spacing={2}><Grid size={{ xs: 12, md: 8 }}><TextField fullWidth label="Nombre completo del destinatario" value={form.recipientName} onChange={(e) => setForm({ ...form, recipientName: e.target.value })} required /></Grid><Grid size={{ xs: 12, md: 4 }}><TextField fullWidth label="Teléfono" value={form.recipientPhone} onChange={(e) => setForm({ ...form, recipientPhone: e.target.value })} /></Grid><Grid size={12}><TextField fullWidth multiline minRows={3} label="Dirección completa del destinatario" value={form.recipientAddress} onChange={(e) => setForm({ ...form, recipientAddress: e.target.value })} required /></Grid></Grid>}
      {step === 2 && <Grid container spacing={3}><Grid size={{ xs: 12, md: 4 }}><TextField select fullWidth label="Servicio postal" value={form.postalServiceId} onChange={(e) => { setForm({ ...form, postalServiceId: e.target.value }); setQuote(null) }} required>{catalog.postalServices.map((item) => <MenuItem key={item.id} value={item.id}>{item.name}</MenuItem>)}</TextField></Grid><Grid size={{ xs: 12, md: 4 }}><TextField select fullWidth label="Destino" value={form.destinationId} onChange={(e) => { setForm({ ...form, destinationId: e.target.value }); setQuote(null) }} required>{catalog.destinations.map((item) => <MenuItem key={item.id} value={item.id}>{item.name}{item.isDomestic ? ' · Nacional' : ''}</MenuItem>)}</TextField></Grid><Grid size={{ xs: 12, md: 4 }}><TextField fullWidth type="number" label="Peso en kilogramos" value={form.weightKg} helperText={Number(form.weightKg) > 0 ? `${Math.round(Number(form.weightKg) * 1000)} gramos` : 'Ejemplo: 0.200 kg'} inputProps={{ min: 0.001, max: 5, step: 0.001 }} onChange={(e) => { setForm({ ...form, weightKg: e.target.value }); setQuote(null) }} required /></Grid><Grid size={12}><Alert severity="info">La tarifa se buscará automáticamente según el servicio, la zona del destino y el rango de peso.</Alert></Grid></Grid>}
      {step === 3 && <Box><Typography variant="h6" color="primary">Servicios suplementarios</Typography><Typography color="text.secondary" sx={{ mb: 2 }}>Selecciona únicamente los servicios adicionales solicitados por el cliente.</Typography><FormGroup>{catalog.supplementaryServices.map((item) => <FormControlLabel key={item.id} control={<Checkbox checked={form.supplementaryServiceIds.includes(item.id)} onChange={() => toggleSupplementary(item.id)} />} label={`${item.name} · ${money(item.price)}`} />)}</FormGroup>{quote && <Alert severity="success" sx={{ mt: 2 }}>Tarifa base encontrada: {money(quote.basePrice)} para el rango {quote.weightBand}.</Alert>}</Box>}
      {step === 4 && quote && <Grid container spacing={3}><Grid size={{ xs: 12, md: 7 }}><Stack spacing={2}><Box><Typography variant="caption" color="text.secondary">REMITENTE</Typography><Typography fontWeight={700}>{form.senderName}</Typography><Typography color="text.secondary">{form.senderAddress}</Typography></Box><Divider /><Box><Typography variant="caption" color="text.secondary">DESTINATARIO</Typography><Typography fontWeight={700}>{form.recipientName}</Typography><Typography color="text.secondary">{form.recipientAddress} · {catalog.destinations.find((x) => x.id === Number(form.destinationId))?.name}</Typography></Box><Divider /><Box><Typography variant="caption" color="text.secondary">SERVICIO</Typography><Typography fontWeight={700}>{catalog.postalServices.find((x) => x.id === Number(form.postalServiceId))?.name} · {Number(form.weightKg).toFixed(3)} kg ({Math.round(Number(form.weightKg) * 1000)} g)</Typography></Box></Stack></Grid><Grid size={{ xs: 12, md: 5 }}><Paper elevation={0} sx={{ p: 3, bgcolor: '#f1f6fa' }}><Box sx={{ display: 'flex', justifyContent: 'space-between' }}><Typography>Tarifa por peso y destino</Typography><Typography>{money(quote.basePrice)}</Typography></Box><Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 1 }}><Typography>Servicios suplementarios</Typography><Typography>{money(quote.supplementaryTotal)}</Typography></Box><Divider sx={{ my: 2 }} /><Box sx={{ display: 'flex', justifyContent: 'space-between' }}><Typography variant="h6">Total</Typography><Typography variant="h4" color="primary">{money(quote.total)}</Typography></Box><TextField select fullWidth label="Forma de pago" value={form.paymentMethodId} onChange={(e) => setForm({ ...form, paymentMethodId: e.target.value })} sx={{ mt: 3 }}>{catalog.paymentMethods.map((item) => <MenuItem key={item.id} value={item.id}>{item.name}</MenuItem>)}</TextField></Paper></Grid></Grid>}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 5 }}><Button disabled={step === 0} onClick={() => { setError(''); setStep((value) => value - 1) }}>Anterior</Button>{step < 4 ? <Button variant="contained" onClick={next}>Continuar</Button> : <Button variant="contained" size="large" disabled={!cash?.isOpen || !form.paymentMethodId} onClick={finish}>Cobrar y generar envío</Button>}</Box>
    </Paper>
    <Typography variant="h6" sx={{ mt: 5, mb: 2 }}>Envíos recientes</Typography><Paper elevation={0} sx={{ border: '1px solid', borderColor: 'divider' }}>{recent.length ? recent.map((sale) => <Box key={sale.id} sx={{ display: 'flex', justifyContent: 'space-between', p: 2, borderBottom: '1px solid', borderColor: 'divider' }}><Box><Typography fontWeight={700}>{sale.shipment?.trackingNumber ?? sale.invoiceNumber}</Typography><Typography variant="caption" color="text.secondary">{sale.shipment ? `${sale.shipment.recipientName} · ${sale.shipment.destination}` : sale.invoiceNumber}</Typography></Box><Typography fontWeight={800}>{money(sale.total)}</Typography></Box>) : <Typography color="text.secondary" sx={{ p: 2 }}>Aún no hay envíos registrados.</Typography>}</Paper>
  </Container>
}
