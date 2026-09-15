import { AppBar, Box, Button, Card, CardContent, Container, Grid, Toolbar, Typography } from '@mui/material'

const modules = [
  ['Ventas', 'Registro de cobros y servicios postales'],
  ['Envíos', 'Envíos postales, etiquetas y seguimiento'],
  ['Caja', 'Apertura, recaudación y cierre diario'],
  ['Inventario', 'Productos, franqueo y asignaciones'],
  ['Apartados', 'Alquileres, adjuntos y morosidad'],
  ['Reportes', 'Informes, estadísticas y conciliaciones'],
]

export default function DashboardPage({ session, onLogout }) {
  return (
    <Box sx={{ minHeight: '100vh' }}>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>COTELNET</Typography>
          <Typography sx={{ mr: 2, display: { xs: 'none', sm: 'block' } }}>{session.fullName}</Typography>
          <Button color="inherit" onClick={onLogout}>Cerrar sesión</Button>
        </Toolbar>
      </AppBar>
      <Container sx={{ py: 5 }}>
        <Typography variant="h4" gutterBottom>Panel principal</Typography>
        <Typography color="text.secondary" sx={{ mb: 4 }}>Base inicial de la nueva plataforma COTELNET.</Typography>
        <Grid container spacing={3}>
          {modules.map(([title, description]) => (
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
    </Box>
  )
}

