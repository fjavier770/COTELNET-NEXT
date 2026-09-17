import PointOfSaleRoundedIcon from '@mui/icons-material/PointOfSaleRounded'
import LocalShippingRoundedIcon from '@mui/icons-material/LocalShippingRounded'
import AccountBalanceWalletRoundedIcon from '@mui/icons-material/AccountBalanceWalletRounded'
import InventoryRoundedIcon from '@mui/icons-material/InventoryRounded'
import MarkunreadMailboxRoundedIcon from '@mui/icons-material/MarkunreadMailboxRounded'
import AssessmentRoundedIcon from '@mui/icons-material/AssessmentRounded'
import ArrowForwardRoundedIcon from '@mui/icons-material/ArrowForwardRounded'
import { Box, Card, CardActionArea, CardContent, Chip, Container, Grid, Typography } from '@mui/material'
import { Link } from 'react-router'

const modules = [
  ['Ventas', 'Registro, tarifa y cobro de servicios postales', 'ventas.access', '/ventas', PointOfSaleRoundedIcon],
  ['Envíos', 'Etiquetas, seguimiento y admisión postal', 'envios.access', null, LocalShippingRoundedIcon],
  ['Caja', 'Apertura, recaudación y cierre diario', 'caja.access', '/caja', AccountBalanceWalletRoundedIcon],
  ['Inventario', 'Productos, franqueo y asignaciones', 'inventario.access', null, InventoryRoundedIcon],
  ['Apartados', 'Alquileres, adjuntos y morosidad', 'apartados.access', null, MarkunreadMailboxRoundedIcon],
  ['Reportes', 'Informes, estadísticas y conciliaciones', 'reportes.access', null, AssessmentRoundedIcon],
]

export default function DashboardPage({ session }) {
  const visibleModules = modules.filter((module) => (session.permissions ?? []).includes(module[2]))
  return (
      <Container maxWidth="xl" sx={{ py: { xs: 3, md: 4 } }}>
        <Box sx={{ mb: 3, p: { xs: 2.5, md: 3.5 }, color: '#fff', borderRadius: 2, background: 'linear-gradient(115deg,#003b70 0%,#075f96 68%,#1685be 100%)', position: 'relative', overflow: 'hidden' }}>
          <Box sx={{ position: 'absolute', width: 230, height: 230, borderRadius: '50%', border: '55px solid rgba(255,255,255,.05)', right: -50, top: -100 }} />
          <Chip label="Sesión operativa" size="small" sx={{ mb: 1.5, bgcolor: 'rgba(255,255,255,.14)', color: '#fff' }} />
          <Typography variant="h4">Bienvenido, {session.fullName}</Typography>
          <Typography sx={{ mt: .75, opacity: .82 }}>Selecciona un módulo para comenzar tu jornada en COTELNET.</Typography>
        </Box>
        <Typography variant="h6" color="primary.dark" sx={{ mb: 2 }}>Módulos disponibles</Typography>
        <Grid container spacing={3}>
          {visibleModules.map(([title, description, , path, Icon]) => (
            <Grid key={title} size={{ xs: 12, sm: 6, md: 4 }}>
              <Card sx={{ height: '100%', borderTop: 4, borderTopColor: 'secondary.main' }}>
                <CardActionArea component={path ? Link : 'div'} to={path || undefined} sx={{ height: '100%' }}><CardContent sx={{ p: 2.75 }}>
                  <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', mb: 2 }}>
                    <Box sx={{ width: 46, height: 46, display: 'grid', placeItems: 'center', bgcolor: '#e7f2f8', color: 'primary.main', borderRadius: 1.5 }}><Box component={Icon} /></Box>
                    {path && <ArrowForwardRoundedIcon color="secondary" />}
                  </Box>
                  <Typography variant="h6" color="primary.dark">{title}</Typography>
                  <Typography color="text.secondary" variant="body2" sx={{ mt: .5 }}>{description}</Typography>
                </CardContent></CardActionArea>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Container>
  )
}
