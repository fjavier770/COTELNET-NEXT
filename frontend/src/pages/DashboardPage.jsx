import { Card, CardContent, Container, Grid, Typography } from '@mui/material'

const modules = [
  ['Ventas', 'Registro de cobros y servicios postales', 'ventas.access'],
  ['Envíos', 'Envíos postales, etiquetas y seguimiento', 'envios.access'],
  ['Caja', 'Apertura, recaudación y cierre diario', 'caja.access'],
  ['Inventario', 'Productos, franqueo y asignaciones', 'inventario.access'],
  ['Apartados', 'Alquileres, adjuntos y morosidad', 'apartados.access'],
  ['Reportes', 'Informes, estadísticas y conciliaciones', 'reportes.access'],
]

export default function DashboardPage({ session }) {
  const visibleModules = modules.filter((module) => (session.permissions ?? []).includes(module[2]))
  return (
      <Container maxWidth="xl" sx={{ py: 5 }}>
        <Typography variant="h4" gutterBottom>Panel principal</Typography>
        <Typography color="text.secondary" sx={{ mb: 4 }}>Módulos habilitados para {session.fullName}.</Typography>
        <Grid container spacing={3}>
          {visibleModules.map(([title, description]) => (
            <Grid key={title} size={{ xs: 12, sm: 6, md: 4 }}>
              <Card sx={{ height: '100%', borderTop: 4, borderColor: 'secondary.main' }}>
                <CardContent>
                  <Typography variant="h6" color="primary">{title}</Typography>
                  <Typography color="text.secondary">{description}</Typography>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Container>
  )
}
