import { useCallback, useEffect, useState } from 'react'
import PersonRoundedIcon from '@mui/icons-material/PersonRounded'
import LocationOnRoundedIcon from '@mui/icons-material/LocationOnRounded'
import Inventory2RoundedIcon from '@mui/icons-material/Inventory2Rounded'
import AddTaskRoundedIcon from '@mui/icons-material/AddTaskRounded'
import PaymentsRoundedIcon from '@mui/icons-material/PaymentsRounded'
import HistoryRoundedIcon from '@mui/icons-material/HistoryRounded'
import PrintRoundedIcon from '@mui/icons-material/PrintRounded'
import CheckCircleRoundedIcon from '@mui/icons-material/CheckCircleRounded'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import { Alert, Box, Button, Checkbox, Chip, Container, Divider, FormControlLabel, FormGroup, Grid, MenuItem, Paper, Stack, Step, StepLabel, Stepper, TextField, Typography } from '@mui/material'
import { Link } from 'react-router'
import SectionPanel from '../components/SectionPanel.jsx'
import { apiRequest } from '../services/api.js'

const money = (value) => new Intl.NumberFormat('es-PA', { style: 'currency', currency: 'PAB' }).format(value ?? 0)
const initialForm = { senderName: '', senderDocument: '', senderCountryCode: 'PA', senderTitle: '', senderIsMinor: false, senderFirstName: '', senderMiddleName: '', senderFirstLastName: '', senderSecondLastName: '', senderPhone: '', senderSecondaryPhone: '', senderEmail: '', senderProvince: '', senderCity: '', senderPostalCode: '', senderStreet: '', senderHouseNumber: '', senderAddress: '', senderFax: '', recipientName: '', recipientPhone: '', recipientAddress: '', postalServiceId: '', destinationId: '', weightKg: '', supplementaryServiceIds: [], paymentMethodId: '' }
const senderTitles = ['Sr.', 'Sra.', 'Srta.', 'Dr.', 'Dra.', 'Lic.', 'Ing.']
const steps = ['Remitente', 'Destinatario', 'Servicio y destino', 'Suplementarios', 'Cobro']
const stepMeta = [
  { title: 'Datos del cliente / remitente', icon: <PersonRoundedIcon fontSize="small" /> },
  { title: 'Datos del destinatario', icon: <LocationOnRoundedIcon fontSize="small" /> },
  { title: 'Servicio postal y destino', icon: <Inventory2RoundedIcon fontSize="small" /> },
  { title: 'Servicios adicionales', icon: <AddTaskRoundedIcon fontSize="small" /> },
  { title: 'Revisión y cobro', icon: <PaymentsRoundedIcon fontSize="small" /> },
]
const safe = (value) => String(value ?? '').replace(/[&<>"']/g, (character) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[character])
const senderFullName = (value) => [value.senderFirstName, value.senderMiddleName, value.senderFirstLastName, value.senderSecondLastName].filter((part) => part?.trim()).join(' ')

function printDocument(title, body, pageSize = 'auto') {
  const popup = window.open('', '_blank', 'width=800,height=900')
  if (!popup) return
  popup.document.write(`<!doctype html><html><head><title>${safe(title)}</title><style>@page{size:${pageSize};margin:0}*{box-sizing:border-box}body{font-family:Arial,sans-serif;color:#10233a;margin:0;background:#fff}h1{color:#003b70;margin:0}h2{margin:8px 0}.muted{color:#607080}.row{display:flex;justify-content:space-between;gap:20px}.box{border:1px solid #ccd5df;border-radius:8px;padding:14px;margin:12px 0}table{width:100%;border-collapse:collapse}th,td{text-align:left;padding:8px;border-bottom:1px solid #ddd}.total{font-size:24px;font-weight:700;color:#003b70}.barcode svg{display:block;width:100%;height:auto}small{font-size:11px}.brand-lockup{display:flex;align-items:center;gap:8px}.brand-lockup .brand-icon{display:grid;place-items:center;width:30px;height:30px;border-radius:7px;background:#ef7d00;color:#fff;font-size:18px;font-weight:900}.brand-lockup strong{display:block;color:#003b70;font-size:19px;line-height:1;letter-spacing:-.5px}.brand-lockup b{display:block;color:#ef7d00;font-size:11px;letter-spacing:1.5px;line-height:1.2}.receipt{width:80mm;margin:0 auto;padding:4mm;font-family:'Courier New',monospace;color:#000;font-size:10.5px;line-height:1.25}.receipt .center{text-align:center}.receipt .receipt-brand{font-family:Arial,sans-serif;font-size:16px;font-weight:900;letter-spacing:.3px}.receipt .receipt-brand span{color:#ef7d00}.receipt .receipt-title{font-size:11px;font-weight:800;letter-spacing:.8px}.receipt .rule{border-top:1px dashed #000;margin:6px 0}.receipt .section{text-align:center;font-weight:700;margin:5px 0}.receipt .line{display:flex;justify-content:space-between;gap:8px}.receipt .line span:first-child{flex:1}.receipt .detail{margin:4px 0 7px}.receipt table{font-size:10px}.receipt th,.receipt td{padding:2px 0;border:0;vertical-align:top}.receipt th:last-child,.receipt td:last-child{text-align:right}.receipt .grand{font-weight:700;font-size:12px}.receipt .barcode{margin:7px auto 0;width:68mm}.label{width:4in;min-height:6in;padding:0.22in;display:flex;flex-direction:column;gap:10px}.label .label-header{display:flex;justify-content:space-between;align-items:flex-start;border-bottom:3px solid #003b70;padding-bottom:9px}.label .format{text-align:right;font-size:10px;color:#526579}.label .format strong{display:block;color:#003b70;font-size:16px;letter-spacing:.7px}.label .tracking{border:2px solid #003b70;border-radius:8px;padding:10px 12px;text-align:center}.label .tracking small{display:block;color:#526579;font-size:10px;font-weight:700;letter-spacing:1.4px}.label .tracking strong{display:block;color:#003b70;font:900 26px/1.15 'Courier New',monospace;letter-spacing:1px}.label .address{border:1px solid #b8c8d8;border-radius:8px;padding:11px 12px}.label .address.recipient{border:3px solid #003b70;padding:10px 11px;min-height:1.48in}.label .address h2{color:#003b70;font-size:17px;line-height:1.15;margin:5px 0}.label .address p{margin:3px 0;font-size:12px;line-height:1.3;white-space:pre-line}.label .address .caption{font-size:10px;color:#526579;font-weight:800;letter-spacing:1px}.label .service-strip{display:flex;justify-content:space-between;gap:8px;border-top:1px solid #b8c8d8;border-bottom:1px solid #b8c8d8;padding:8px 0;font-size:11px}.label .service-strip strong{color:#003b70}.label .label-footer{margin-top:auto;text-align:center}.label .label-footer .barcode{margin:7px auto 0;width:100%}.label .label-footer .barcode svg{max-height:1.05in}.label .fine{font-size:9px;color:#526579}@media print{body{-webkit-print-color-adjust:exact;print-color-adjust:exact}}
  </style></head><body>${body}<script>window.onload=()=>window.print()</script></body></html>`)
  popup.document.close()
}

function printInvoice(sale) {
  const shipment = sale.shipment
  const receipt = sale.receipt ?? {}
  printDocument(sale.invoiceNumber, `<main class="receipt"><header class="center"><div class="receipt-brand"><span>CORREOS</span> PANAMÁ</div><div>Dirección Nacional de Correos y Telégrafos</div><div class="receipt-title">RECIBO DE VENTA · FORMATO CN-04</div></header><div class="rule"></div><div class="line"><span>RECIBO:</span><strong>${safe(sale.invoiceNumber)}</strong></div><div class="line"><span>FECHA:</span><span>${safe(new Date(sale.createdAtUtc).toLocaleString('es-PA'))}</span></div><div class="line"><span>CAJA:</span><span>${safe(receipt.terminalCode)}</span></div><div class="line"><span>OFICINISTA:</span><span>${safe(receipt.cashier)}</span></div><div class="line"><span>ESTAFETA:</span><strong>${safe(receipt.officeCode)} · ${safe(receipt.officeName)}</strong></div><div class="rule"></div><div class="section">REMITENTE</div><div>${safe(shipment.senderName)}</div><div>Cédula/Pasaporte: ${safe(shipment.senderDocument || 'N/D')}</div><div>${safe(shipment.senderAddress)}</div><div>Tel.: ${safe(shipment.senderPhone || 'N/D')}</div><div class="rule"></div><div class="section">ENVÍO POSTAL</div><div class="detail"><strong>DESTINATARIO:</strong> ${safe(shipment.recipientName)}<br><strong>DIRECCIÓN:</strong> ${safe(shipment.recipientAddress)}<br><strong>DESTINO:</strong> ${safe(shipment.destination)}<br><strong>SERVICIO:</strong> ${safe(shipment.postalService)}<br><strong>PESO:</strong> ${(shipment.weightGrams / 1000).toFixed(3)} kg<br><strong>S10:</strong> ${safe(shipment.trackingNumber)}</div><table><thead><tr><th>CANT. DESCRIPCIÓN</th><th>IMPORTE</th></tr></thead><tbody>${sale.lines.map((line) => `<tr><td>${line.quantity} ${safe(line.description)}</td><td>B/.${Number(line.total).toFixed(2)}</td></tr>`).join('')}</tbody></table><div class="rule"></div><div class="line grand"><span>TOTAL:</span><strong>B/.${Number(sale.total).toFixed(2)}</strong></div>${sale.payments.map((payment) => `<div class="line"><span>${safe(payment.paymentMethod)}:</span><span>B/.${Number(payment.amount).toFixed(2)}</span></div>`).join('')}<div class="rule"></div><div class="center"><strong>DOCUMENTO NO FISCAL</strong><p>Conserve este recibo para cualquier reclamo.</p><div class="barcode">${shipment.barcodeSvg}</div><strong>${safe(shipment.trackingNumber)}</strong><p>Tu correo seguro y económico</p></div></main>`, '80mm auto')
}

function printLabel(sale) {
  const shipment = sale.shipment
  printDocument(shipment.trackingNumber, `<main class="label"><header class="label-header"><div class="brand-lockup"><span class="brand-icon">✉</span><div><strong>CORREOS</strong><b>PANAMÁ</b></div></div><div class="format"><strong>CN-04</strong>ETIQUETA POSTAL<br>S10 · UPU</div></header><div class="tracking"><small>CÓDIGO DE ENVÍO</small><strong>${safe(shipment.trackingNumber)}</strong></div><section class="address recipient"><div class="caption">DESTINATARIO / PARA</div><h2>${safe(shipment.recipientName)}</h2><p>${safe(shipment.recipientAddress)}<br><strong>${safe(shipment.destination)}</strong>${shipment.recipientPhone ? `<br>Tel. ${safe(shipment.recipientPhone)}` : ''}</p></section><section class="address"><div class="caption">REMITENTE / DE</div><h2>${safe(shipment.senderName)}</h2><p>${safe(shipment.senderAddress)}${shipment.senderPhone ? `<br>Tel. ${safe(shipment.senderPhone)}` : ''}</p></section><div class="service-strip"><span><strong>SERVICIO</strong><br>${safe(shipment.postalService)}</span><span><strong>PESO</strong><br>${(shipment.weightGrams / 1000).toFixed(3)} kg</span><span><strong>ESTAFETA</strong><br>${safe(sale.receipt?.officeCode || 'N/D')}</span></div><div class="fine">${shipment.supplementaryServices.length ? `Adicionales: ${shipment.supplementaryServices.map((x) => safe(x.name)).join(' · ')}` : 'Manipular según condiciones del servicio postal.'}</div><footer class="label-footer"><div class="barcode">${shipment.barcodeSvg}</div><div class="fine">Código S10 UPU · Conservar para rastreo</div></footer></main>`, '4in 6in')
}

function SummaryLine({ label, value, strong = false }) {
  return <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 2, py: .65 }}><Typography variant="body2" color={strong ? 'text.primary' : 'text.secondary'} fontWeight={strong ? 800 : 600}>{label}</Typography><Typography variant="body2" fontWeight={strong ? 900 : 750} color={strong ? 'secondary.dark' : 'text.primary'} textAlign="right">{value}</Typography></Box>
}

export default function SalesPage({ session }) {
  const [catalog, setCatalog] = useState({ paymentMethods: [], postalServices: [], destinations: [], supplementaryServices: [], serviceWeightLimits: [] })
  const [cash, setCash] = useState(null); const [recent, setRecent] = useState([]); const [form, setForm] = useState(initialForm); const [step, setStep] = useState(0); const [quote, setQuote] = useState(null); const [completed, setCompleted] = useState(null); const [error, setError] = useState(''); const [senderLookup, setSenderLookup] = useState('')
  const load = useCallback(async () => {
    try {
      const [data, current, sales] = await Promise.all([apiRequest('/sales/catalog', session.accessToken), apiRequest('/sales/cash/current', session.accessToken), apiRequest('/sales/recent', session.accessToken)])
      setCatalog(data); setCash(current); setRecent(sales); setForm((currentForm) => ({ ...currentForm, paymentMethodId: currentForm.paymentMethodId || data.paymentMethods[0]?.id || '' })); setError('')
    } catch (err) { setError(err.message) }
  }, [session.accessToken])
  useEffect(() => { load() }, [load])

  async function lookupSender() {
    if (form.senderDocument.trim().length < 4) return
    setSenderLookup('Buscando remitente…')
    try {
      const sender = await apiRequest(`/sales/senders/${encodeURIComponent(form.senderDocument.trim())}`, session.accessToken)
      if (!sender) { setSenderLookup('Documento nuevo · los datos se guardarán al completar el envío.'); return }
      setForm((value) => ({ ...value, senderDocument: sender.documentNumber, senderCountryCode: sender.countryCode, senderTitle: sender.title, senderIsMinor: sender.isMinor, senderFirstName: sender.firstName, senderMiddleName: sender.middleName, senderFirstLastName: sender.firstLastName, senderSecondLastName: sender.secondLastName, senderPhone: sender.primaryPhone, senderSecondaryPhone: sender.secondaryPhone, senderEmail: sender.email, senderProvince: sender.province, senderCity: sender.city, senderPostalCode: sender.postalCode, senderStreet: sender.street, senderHouseNumber: sender.houseNumber, senderAddress: sender.address, senderFax: sender.fax, senderName: [sender.firstName, sender.middleName, sender.firstLastName, sender.secondLastName].filter(Boolean).join(' ') }))
      setSenderLookup('Remitente encontrado · puedes actualizar sus datos antes de continuar.')
    } catch (err) { setError(err.message); setSenderLookup('') }
  }

  const selectedService = catalog.postalServices.find((item) => item.id === Number(form.postalServiceId))
  const selectedDestination = catalog.destinations.find((item) => item.id === Number(form.destinationId))
  const currentMaximumWeight = catalog.serviceWeightLimits.find((item) => item.postalServiceId === Number(form.postalServiceId) && item.destinationZone === selectedDestination?.zone)?.maximumWeightGrams

  function validateCurrentStep() {
    if (step === 0 && (!form.senderDocument.trim() || !form.senderFirstName.trim() || !form.senderFirstLastName.trim() || !form.senderAddress.trim())) return 'Completa la cédula o pasaporte, primer nombre, primer apellido y dirección del remitente.'
    if (step === 1 && (!form.recipientName.trim() || !form.recipientAddress.trim())) return 'Completa el nombre y la dirección del destinatario.'
    if (step === 2 && (!form.postalServiceId || !form.destinationId || Number(form.weightKg) <= 0)) return 'Selecciona el servicio, destino e indica un peso válido.'
    if (step === 2 && currentMaximumWeight && Math.round(Number(form.weightKg) * 1000) > currentMaximumWeight) return `El peso máximo permitido es ${(currentMaximumWeight / 1000).toFixed(3)} kg.`
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
      const result = await apiRequest('/sales/shipments', session.accessToken, { method: 'POST', body: JSON.stringify({ ...form, senderName: senderFullName(form), postalServiceId: Number(form.postalServiceId), destinationId: Number(form.destinationId), weightGrams: Math.round(Number(form.weightKg) * 1000), paymentMethodId: Number(form.paymentMethodId) }) })
      setCompleted(result); await load()
    } catch (err) { setError(err.message) }
  }

  function reset() { setForm({ ...initialForm, paymentMethodId: catalog.paymentMethods[0]?.id || '' }); setStep(0); setQuote(null); setCompleted(null); setError(''); setSenderLookup('') }

  if (completed) return <Container maxWidth="md" sx={{ py: 5 }}><Paper elevation={0} sx={{ overflow: 'hidden', textAlign: 'center' }}><Box sx={{ p: { xs: 3, md: 5 }, color: '#fff', background: 'linear-gradient(120deg,#075f96,#1685be)' }}><CheckCircleRoundedIcon sx={{ fontSize: 54, color: '#8ee0b9' }} /><Typography variant="overline" sx={{ display: 'block', mt: 1, opacity: .8 }}>Envío registrado correctamente</Typography><Typography variant="h3" sx={{ mt: 1 }}>{completed.shipment.trackingNumber}</Typography><Typography sx={{ mt: 1, opacity: .85 }}>Factura {completed.invoiceNumber} · Total {money(completed.total)}</Typography></Box><Box sx={{ p: { xs: 3, md: 4 } }}><Grid container spacing={2}><Grid size={{ xs: 12, sm: 6 }}><Button fullWidth variant="contained" size="large" startIcon={<PrintRoundedIcon />} onClick={() => printLabel(completed)}>Imprimir etiqueta</Button></Grid><Grid size={{ xs: 12, sm: 6 }}><Button fullWidth variant="outlined" size="large" startIcon={<PrintRoundedIcon />} onClick={() => printInvoice(completed)}>Imprimir factura</Button></Grid></Grid><Button sx={{ mt: 3 }} onClick={reset}>Registrar otro envío</Button></Box></Paper></Container>

  return <Container maxWidth={false} sx={{ py: { xs: 2.5, md: 3 }, px: { xs: 1.5, sm: 2.5, xl: 3.5 } }}>
    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 2, mb: 2, flexWrap: 'wrap' }}>
      <Box><Typography variant="overline" color="secondary.main" fontWeight={900}>Módulo oficinista de ventas</Typography><Typography variant="h4" color="primary.dark">Admisión de envíos postales</Typography><Typography color="text.secondary" variant="body2">Registro, tarifa, suplementarios y cobro en una sola operación.</Typography></Box>
      <Chip label={cash?.isOpen ? `Caja abierta · ${cash.terminal}` : 'Caja cerrada'} color={cash?.isOpen ? 'success' : 'default'} variant={cash?.isOpen ? 'filled' : 'outlined'} />
    </Box>
    {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
    {!cash?.isOpen && <Alert severity="warning" sx={{ mb: 2 }} action={(session.permissions ?? []).includes('caja.access') ? <Button component={Link} to="/caja" color="inherit">Ir a caja</Button> : null}>Debes abrir una caja antes de registrar envíos.</Alert>}
    <Paper elevation={0} sx={{ px: { xs: 1, md: 2 }, py: 1.5, mb: 2, overflowX: 'auto' }}>
      <Stepper activeStep={step} alternativeLabel sx={{ minWidth: { xs: 650, md: 0 }, '& .MuiStepLabel-label': { fontSize: 12, fontWeight: 700 }, '& .MuiStepIcon-root.Mui-active, & .MuiStepIcon-root.Mui-completed': { color: 'secondary.main' } }}>{steps.map((label) => <Step key={label}><StepLabel>{label}</StepLabel></Step>)}</Stepper>
    </Paper>
    <Grid container spacing={2} alignItems="stretch">
      <Grid size={{ xs: 12, lg: 8 }}>
        <SectionPanel title={stepMeta[step].title.toUpperCase()} icon={stepMeta[step].icon} sx={{ height: '100%' }}>
          {step === 0 && <Grid container spacing={2}>
            <Grid size={{ xs: 12, md: 5 }}><TextField fullWidth label="Cédula o pasaporte" value={form.senderDocument} onChange={(e) => { setForm({ ...form, senderDocument: e.target.value.toUpperCase() }); setSenderLookup('') }} required /></Grid>
            <Grid size={{ xs: 12, sm: 6, md: 3 }}><Button fullWidth variant="outlined" startIcon={<SearchRoundedIcon />} onClick={lookupSender} sx={{ height: 40 }}>Buscar remitente</Button></Grid>
            <Grid size={{ xs: 12, sm: 6, md: 4 }}><FormControlLabel control={<Checkbox checked={form.senderIsMinor} onChange={(e) => setForm({ ...form, senderIsMinor: e.target.checked })} />} label="Menor de edad" /></Grid>
            {senderLookup && <Grid size={12}><Alert severity={senderLookup.startsWith('Remitente encontrado') ? 'success' : 'info'}>{senderLookup}</Alert></Grid>}
            <Grid size={{ xs: 12, sm: 4, md: 3 }}><TextField fullWidth label="País de origen (ISO)" value={form.senderCountryCode} inputProps={{ maxLength: 2 }} onChange={(e) => setForm({ ...form, senderCountryCode: e.target.value.toUpperCase().replace(/[^A-Z]/g, '').slice(0, 2) })} helperText="CDS: código ISO de 2 letras" required /></Grid>
            <Grid size={{ xs: 12, sm: 4, md: 3 }}><TextField select fullWidth label="Título" value={form.senderTitle} onChange={(e) => setForm({ ...form, senderTitle: e.target.value })}><MenuItem value="">Sin título</MenuItem>{senderTitles.map((title) => <MenuItem key={title} value={title}>{title}</MenuItem>)}</TextField></Grid>
            <Grid size={{ xs: 12, sm: 8, md: 5 }}><TextField fullWidth label="Primer nombre" value={form.senderFirstName} onChange={(e) => setForm({ ...form, senderFirstName: e.target.value })} required /></Grid>
            <Grid size={{ xs: 12, md: 4 }}><TextField fullWidth label="Segundo nombre" value={form.senderMiddleName} onChange={(e) => setForm({ ...form, senderMiddleName: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, md: 6 }}><TextField fullWidth label="Primer apellido" value={form.senderFirstLastName} onChange={(e) => setForm({ ...form, senderFirstLastName: e.target.value })} required /></Grid>
            <Grid size={{ xs: 12, md: 6 }}><TextField fullWidth label="Segundo apellido" value={form.senderSecondLastName} onChange={(e) => setForm({ ...form, senderSecondLastName: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, md: 6 }}><TextField fullWidth label="Teléfono principal" value={form.senderPhone} helperText="Nacional: 0000-0000 · Internacional: +00 000 0000" onChange={(e) => setForm({ ...form, senderPhone: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, md: 6 }}><TextField fullWidth label="Teléfono secundario" value={form.senderSecondaryPhone} onChange={(e) => setForm({ ...form, senderSecondaryPhone: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, md: 8 }}><TextField fullWidth type="email" label="Correo electrónico" value={form.senderEmail} onChange={(e) => setForm({ ...form, senderEmail: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, md: 4 }}><TextField fullWidth label="Fax" value={form.senderFax} onChange={(e) => setForm({ ...form, senderFax: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, md: 4 }}><TextField fullWidth label="Provincia" value={form.senderProvince} onChange={(e) => setForm({ ...form, senderProvince: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, md: 4 }}><TextField fullWidth label="Ciudad" value={form.senderCity} onChange={(e) => setForm({ ...form, senderCity: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, md: 4 }}><TextField fullWidth label="Código postal" value={form.senderPostalCode} onChange={(e) => setForm({ ...form, senderPostalCode: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, md: 8 }}><TextField fullWidth label="Calle" value={form.senderStreet} onChange={(e) => setForm({ ...form, senderStreet: e.target.value })} /></Grid>
            <Grid size={{ xs: 12, md: 4 }}><TextField fullWidth label="Número de casa" value={form.senderHouseNumber} onChange={(e) => setForm({ ...form, senderHouseNumber: e.target.value })} /></Grid>
            <Grid size={12}><TextField fullWidth multiline minRows={3} label="Dirección completa" value={form.senderAddress} onChange={(e) => setForm({ ...form, senderAddress: e.target.value })} required /></Grid>
          </Grid>}
          {step === 1 && <Grid container spacing={2}><Grid size={{ xs: 12, md: 8 }}><TextField fullWidth label="Nombre completo del destinatario" value={form.recipientName} onChange={(e) => setForm({ ...form, recipientName: e.target.value })} required /></Grid><Grid size={{ xs: 12, md: 4 }}><TextField fullWidth label="Teléfono" value={form.recipientPhone} onChange={(e) => setForm({ ...form, recipientPhone: e.target.value })} /></Grid><Grid size={12}><TextField fullWidth multiline minRows={4} label="Dirección completa del destinatario" value={form.recipientAddress} onChange={(e) => setForm({ ...form, recipientAddress: e.target.value })} required /></Grid></Grid>}
          {step === 2 && <Grid container spacing={2}><Grid size={{ xs: 12, md: 6 }}><TextField select fullWidth label="Servicio postal" value={form.postalServiceId} onChange={(e) => { setForm({ ...form, postalServiceId: e.target.value, supplementaryServiceIds: [] }); setQuote(null) }} required>{catalog.postalServices.map((item) => <MenuItem key={item.id} value={item.id}>{item.name}</MenuItem>)}</TextField></Grid><Grid size={{ xs: 12, md: 6 }}><TextField select fullWidth label="País o destino" value={form.destinationId} onChange={(e) => { setForm({ ...form, destinationId: e.target.value, supplementaryServiceIds: [] }); setQuote(null) }} required>{catalog.destinations.map((item) => <MenuItem key={item.id} value={item.id}>{item.name}{item.isDomestic ? ' · Nacional' : ''}</MenuItem>)}</TextField></Grid><Grid size={{ xs: 12, md: 5 }}><TextField fullWidth type="number" label="Peso (kg)" value={form.weightKg} helperText={currentMaximumWeight ? `Formato 0.000 · máximo ${(currentMaximumWeight / 1000).toFixed(3)} kg` : 'Formato requerido: 0.000 kg'} inputProps={{ min: 0.001, max: currentMaximumWeight ? currentMaximumWeight / 1000 : undefined, step: 0.001 }} onBlur={() => Number(form.weightKg) > 0 && setForm((value) => ({ ...value, weightKg: Number(value.weightKg).toFixed(3) }))} onChange={(e) => { setForm({ ...form, weightKg: e.target.value, supplementaryServiceIds: [] }); setQuote(null) }} required /></Grid><Grid size={{ xs: 12, md: 7 }}><Alert severity="info">La tarifa se calcula según servicio, destino, vía, grupo y rango de peso configurado.</Alert></Grid></Grid>}
          {step === 3 && <Box><Typography color="text.secondary" variant="body2" sx={{ mb: 2 }}>Selecciona los servicios adicionales permitidos para la tarifa encontrada.</Typography>{quote?.supplementaryServices.length ? <FormGroup>{quote.supplementaryServices.map((item) => <FormControlLabel key={item.id} control={<Checkbox checked={form.supplementaryServiceIds.includes(item.id)} onChange={() => toggleSupplementary(item.id)} />} label={<Box sx={{ display: 'flex', gap: 1.5 }}><Typography fontWeight={750}>{item.name}</Typography><Typography color="secondary.dark" fontWeight={850}>{money(item.price)}</Typography></Box>} />)}</FormGroup> : <Alert severity="info">Esta tarifa no tiene servicios suplementarios asociados.</Alert>}{quote && <Alert severity="success" sx={{ mt: 2 }}>Tarifa base encontrada: {money(quote.basePrice)} · rango {quote.weightBand}.</Alert>}</Box>}
          {step === 4 && quote && <Grid container spacing={3}><Grid size={{ xs: 12, md: 7 }}><Stack spacing={1.5}><Box><Typography variant="caption" color="text.secondary" fontWeight={800}>REMITENTE</Typography><Typography fontWeight={800}>{senderFullName(form)}</Typography><Typography color="text.secondary" variant="body2">{form.senderDocument} · {form.senderAddress}</Typography></Box><Divider /><Box><Typography variant="caption" color="text.secondary" fontWeight={800}>DESTINATARIO</Typography><Typography fontWeight={800}>{form.recipientName}</Typography><Typography color="text.secondary" variant="body2">{form.recipientAddress} · {selectedDestination?.name}</Typography></Box><Divider /><Box><Typography variant="caption" color="text.secondary" fontWeight={800}>SERVICIO</Typography><Typography fontWeight={800}>{selectedService?.name}</Typography><Typography color="text.secondary" variant="body2">{Number(form.weightKg).toFixed(3)} kg · {quote.weightBand}</Typography></Box></Stack></Grid><Grid size={{ xs: 12, md: 5 }}><TextField select fullWidth label="Forma de pago" value={form.paymentMethodId} onChange={(e) => setForm({ ...form, paymentMethodId: e.target.value })}>{catalog.paymentMethods.map((item) => <MenuItem key={item.id} value={item.id}>{item.name}</MenuItem>)}</TextField></Grid></Grid>}
          <Divider sx={{ my: 2.5 }} />
          <Box sx={{ display: 'flex', justifyContent: 'space-between' }}><Button disabled={step === 0} onClick={() => { setError(''); setStep((value) => value - 1) }}>Anterior</Button>{step < 4 ? <Button variant="contained" onClick={next}>Continuar</Button> : <Button variant="contained" color="secondary" size="large" disabled={!cash?.isOpen || !form.paymentMethodId} onClick={finish}>Cobrar y generar envío</Button>}</Box>
        </SectionPanel>
      </Grid>
      <Grid size={{ xs: 12, lg: 4 }}>
        <Stack spacing={2} sx={{ height: '100%' }}>
          <SectionPanel title="RESUMEN DE ADMISIÓN" icon={<PaymentsRoundedIcon fontSize="small" />}>
            <SummaryLine label="Servicio" value={selectedService?.name || 'Pendiente'} />
            <SummaryLine label="Destino" value={selectedDestination?.name || 'Pendiente'} />
            <SummaryLine label="Peso" value={form.weightKg ? `${Number(form.weightKg).toFixed(3)} kg` : '0.000 kg'} />
            <Divider sx={{ my: 1 }} />
            <SummaryLine label="Tarifa postal" value={money(quote?.basePrice)} />
            <SummaryLine label="Suplementarios" value={money(quote?.supplementaryTotal)} />
            <Divider sx={{ my: 1 }} />
            <SummaryLine label="TOTAL B/." value={money(quote?.total)} strong />
          </SectionPanel>
          <SectionPanel title="ENVÍOS RECIENTES" icon={<HistoryRoundedIcon fontSize="small" />} sx={{ flex: 1 }}>
            {recent.length ? recent.slice(0, 6).map((sale, index) => <Box key={sale.id} sx={{ display: 'flex', justifyContent: 'space-between', gap: 1.5, py: 1.15, borderBottom: index < Math.min(recent.length, 6) - 1 ? '1px solid' : 0, borderColor: 'divider' }}><Box sx={{ minWidth: 0 }}><Typography variant="body2" fontWeight={800} color="primary.dark" noWrap>{sale.shipment?.trackingNumber ?? sale.invoiceNumber}</Typography><Typography variant="caption" color="text.secondary" noWrap sx={{ display: 'block' }}>{sale.shipment ? `${sale.shipment.recipientName} · ${sale.shipment.destination}` : sale.invoiceNumber}</Typography></Box><Typography variant="body2" fontWeight={900}>{money(sale.total)}</Typography></Box>) : <Typography color="text.secondary" variant="body2">Aún no hay envíos registrados.</Typography>}
          </SectionPanel>
        </Stack>
      </Grid>
    </Grid>
  </Container>
}
