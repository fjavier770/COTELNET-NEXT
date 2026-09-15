import { useState } from 'react'
import { AppBar, Box, Button, Divider, Drawer, List, ListItemButton, ListItemText, Toolbar, Typography } from '@mui/material'
import { Link, Outlet, useLocation } from 'react-router'

const drawerWidth = 250
const navigation = [
  { label: 'Panel principal', path: '/', permission: 'dashboard.view' },
  { label: 'Ventas', path: '/ventas', permission: 'ventas.access' },
  { label: 'Caja', path: '/caja', permission: 'caja.access' },
  { label: 'Usuarios', path: '/administracion/usuarios', permission: 'users.manage' },
  { label: 'Roles y permisos', path: '/administracion/roles', permission: 'roles.manage' },
  { label: 'Estafetas', path: '/administracion/estafetas', permission: 'estafetas.manage' },
  { label: 'Terminales', path: '/administracion/terminales', permission: 'terminals.manage' },
]

export default function AppShell({ session, onLogout }) {
  const [open, setOpen] = useState(false)
  const location = useLocation()
  const permissions = session.permissions ?? []
  const items = navigation.filter((item) => permissions.includes(item.permission))

  const menu = (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <Toolbar><Typography variant="h6" color="primary">COTELNET</Typography></Toolbar>
      <Divider />
      <List sx={{ px: 1 }}>
        {items.map((item) => (
          <ListItemButton key={item.path} component={Link} to={item.path} selected={location.pathname === item.path} onClick={() => setOpen(false)}>
            <ListItemText primary={item.label} />
          </ListItemButton>
        ))}
      </List>
      <Box sx={{ mt: 'auto', p: 2 }}>
        <Typography variant="body2" fontWeight={700}>{session.fullName}</Typography>
        <Typography variant="caption" color="text.secondary">{session.role}</Typography>
      </Box>
    </Box>
  )

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <AppBar position="fixed" sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}>
        <Toolbar>
          <Button color="inherit" onClick={() => setOpen(true)} sx={{ display: { md: 'none' }, mr: 1 }}>Menú</Button>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>COTELNET</Typography>
          <Button color="inherit" onClick={onLogout}>Cerrar sesión</Button>
        </Toolbar>
      </AppBar>
      <Drawer variant="temporary" open={open} onClose={() => setOpen(false)} sx={{ display: { xs: 'block', md: 'none' }, '& .MuiDrawer-paper': { width: drawerWidth } }}>{menu}</Drawer>
      <Drawer variant="permanent" sx={{ display: { xs: 'none', md: 'block' }, width: drawerWidth, '& .MuiDrawer-paper': { width: drawerWidth } }} open>{menu}</Drawer>
      <Box component="main" sx={{ flexGrow: 1, minWidth: 0, pt: 8 }}><Outlet /></Box>
    </Box>
  )
}
