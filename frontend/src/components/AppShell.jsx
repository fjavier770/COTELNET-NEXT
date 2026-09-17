import { useState } from 'react'
import DashboardRoundedIcon from '@mui/icons-material/DashboardRounded'
import PointOfSaleRoundedIcon from '@mui/icons-material/PointOfSaleRounded'
import AccountBalanceWalletRoundedIcon from '@mui/icons-material/AccountBalanceWalletRounded'
import PeopleAltRoundedIcon from '@mui/icons-material/PeopleAltRounded'
import AdminPanelSettingsRoundedIcon from '@mui/icons-material/AdminPanelSettingsRounded'
import LocationOnRoundedIcon from '@mui/icons-material/LocationOnRounded'
import ComputerRoundedIcon from '@mui/icons-material/ComputerRounded'
import Inventory2RoundedIcon from '@mui/icons-material/Inventory2Rounded'
import MenuRoundedIcon from '@mui/icons-material/MenuRounded'
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded'
import CircleIcon from '@mui/icons-material/Circle'
import { AppBar, Avatar, Box, Button, Divider, Drawer, IconButton, List, ListItemButton, ListItemIcon, ListItemText, Toolbar, Tooltip, Typography } from '@mui/material'
import { Link, Outlet, useLocation } from 'react-router'
import BrandMark from './BrandMark.jsx'

const drawerWidth = 248
const navigation = [
  { label: 'Panel principal', path: '/', permission: 'dashboard.view', icon: DashboardRoundedIcon, group: 'Operación' },
  { label: 'Ventas postales', path: '/ventas', permission: 'ventas.access', icon: PointOfSaleRoundedIcon, group: 'Operación' },
  { label: 'Control de caja', path: '/caja', permission: 'caja.access', icon: AccountBalanceWalletRoundedIcon, group: 'Operación' },
  { label: 'Usuarios', path: '/administracion/usuarios', permission: 'users.manage', icon: PeopleAltRoundedIcon, group: 'Administración' },
  { label: 'Roles y permisos', path: '/administracion/roles', permission: 'roles.manage', icon: AdminPanelSettingsRoundedIcon, group: 'Administración' },
  { label: 'Estafetas', path: '/administracion/estafetas', permission: 'estafetas.manage', icon: LocationOnRoundedIcon, group: 'Administración' },
  { label: 'Terminales', path: '/administracion/terminales', permission: 'terminals.manage', icon: ComputerRoundedIcon, group: 'Administración' },
  { label: 'Servicios y tarifas', path: '/administracion/servicios', permission: 'services.manage', icon: Inventory2RoundedIcon, group: 'Administración' },
]

export default function AppShell({ session, onLogout }) {
  const [open, setOpen] = useState(false)
  const location = useLocation()
  const permissions = session.permissions ?? []
  const items = navigation.filter((item) => permissions.includes(item.permission))
  const groups = [...new Set(items.map((item) => item.group))]
  const initials = session.fullName?.split(' ').slice(0, 2).map((word) => word[0]).join('').toUpperCase() || 'CU'

  const menu = (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column', bgcolor: '#fff' }}>
      <Toolbar sx={{ minHeight: '72px !important', px: '18px !important' }}><BrandMark /></Toolbar>
      <Divider />
      <Box sx={{ px: 2, py: 1.5, bgcolor: '#f6f9fb', borderBottom: '1px solid', borderColor: 'divider' }}>
        <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 700 }}>MÓDULO OPERATIVO</Typography>
        <Typography variant="body2" color="primary.dark" fontWeight={800}>Oficinista de ventas</Typography>
      </Box>
      <List sx={{ px: 1.25, py: 1.5 }}>
        {groups.map((group) => (
          <Box key={group} sx={{ mb: 1.5 }}>
            <Typography variant="caption" sx={{ display: 'block', px: 1.5, py: .75, color: '#8294a1', fontSize: 10.5, fontWeight: 850, letterSpacing: 1.1 }}>{group.toUpperCase()}</Typography>
            {items.filter((item) => item.group === group).map((item) => {
              const Icon = item.icon
              const selected = location.pathname === item.path
              return (
                <ListItemButton key={item.path} component={Link} to={item.path} selected={selected} onClick={() => setOpen(false)} sx={{ mb: .4 }}>
                  <ListItemIcon sx={{ minWidth: 38, color: selected ? 'primary.main' : '#708492' }}><Icon fontSize="small" /></ListItemIcon>
                  <ListItemText primary={item.label} primaryTypographyProps={{ fontSize: 13.5, fontWeight: selected ? 800 : 650 }} />
                </ListItemButton>
              )
            })}
          </Box>
        ))}
      </List>
      <Box sx={{ mt: 'auto', p: 1.5, borderTop: '1px solid', borderColor: 'divider' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.25, p: 1, bgcolor: '#f4f8fa', borderRadius: 1.5 }}>
          <Avatar sx={{ width: 36, height: 36, bgcolor: 'primary.main', fontSize: 13, fontWeight: 800 }}>{initials}</Avatar>
          <Box sx={{ minWidth: 0, flex: 1 }}>
            <Typography variant="body2" fontWeight={800} noWrap>{session.fullName}</Typography>
            <Typography variant="caption" color="text.secondary" noWrap>{session.role}</Typography>
          </Box>
        </Box>
      </Box>
    </Box>
  )

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <AppBar position="fixed" elevation={0} sx={{ zIndex: (theme) => theme.zIndex.drawer + 1, ml: { md: `${drawerWidth}px` }, width: { md: `calc(100% - ${drawerWidth}px)` }, bgcolor: '#fff', color: 'text.primary', border: 0, borderBottom: '1px solid', borderColor: 'divider' }}>
        <Toolbar sx={{ minHeight: '64px !important', px: { xs: 1.5, md: 3 } }}>
          <IconButton onClick={() => setOpen(true)} sx={{ display: { md: 'none' }, mr: 1 }}><MenuRoundedIcon /></IconButton>
          <Box sx={{ flexGrow: 1 }}>
            <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 700 }}>SISTEMA POSTAL NACIONAL</Typography>
            <Typography variant="subtitle1" color="primary.dark" sx={{ fontWeight: 850, lineHeight: 1.15 }}>COTELNET · Plataforma operativa</Typography>
          </Box>
          <Box sx={{ display: { xs: 'none', sm: 'flex' }, alignItems: 'center', gap: .75, mr: 2, px: 1.25, py: .7, bgcolor: '#eef8f4', borderRadius: 1 }}>
            <CircleIcon sx={{ fontSize: 9, color: 'success.main' }} />
            <Typography variant="caption" color="success.dark" fontWeight={800}>Conectado</Typography>
          </Box>
          <Tooltip title="Cerrar sesión"><Button color="inherit" onClick={onLogout} startIcon={<LogoutRoundedIcon />} sx={{ color: 'primary.dark' }}><Box component="span" sx={{ display: { xs: 'none', sm: 'inline' } }}>Salir</Box></Button></Tooltip>
        </Toolbar>
      </AppBar>
      <Drawer variant="temporary" open={open} onClose={() => setOpen(false)} ModalProps={{ keepMounted: true }} sx={{ display: { xs: 'block', md: 'none' }, '& .MuiDrawer-paper': { width: drawerWidth } }}>{menu}</Drawer>
      <Drawer variant="permanent" sx={{ display: { xs: 'none', md: 'block' }, width: drawerWidth, flexShrink: 0, '& .MuiDrawer-paper': { width: drawerWidth, borderRightColor: '#cbd8e0' } }} open>{menu}</Drawer>
      <Box component="main" sx={{ flexGrow: 1, minWidth: 0, pt: 8, pb: 4.5 }}><Outlet /></Box>
      <Box sx={{ position: 'fixed', left: { xs: 0, md: drawerWidth }, right: 0, bottom: 0, zIndex: 1100, height: 30, px: 2, display: 'flex', alignItems: 'center', justifyContent: 'space-between', bgcolor: '#f7f9fa', borderTop: '1px solid', borderColor: '#c9d4db' }}>
        <Typography variant="caption" color="primary.dark" fontWeight={750}>● Conectado · {session.fullName}</Typography>
        <Typography variant="caption" color="text.secondary" sx={{ display: { xs: 'none', sm: 'block' } }}>Correos Panamá · COTELNET-NEXT</Typography>
      </Box>
    </Box>
  )
}
