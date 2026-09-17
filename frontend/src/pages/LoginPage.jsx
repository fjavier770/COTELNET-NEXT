import { useState } from 'react'
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Container,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import LockRoundedIcon from '@mui/icons-material/LockRounded'
import VerifiedUserRoundedIcon from '@mui/icons-material/VerifiedUserRounded'
import BrandMark from '../components/BrandMark.jsx'
import { login } from '../services/api.js'

export default function LoginPage({ onAuthenticated }) {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setLoading(true)
    try {
      const session = await login(username, password)
      onAuthenticated(session)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <Box sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center', position: 'relative', overflow: 'hidden', background: 'linear-gradient(132deg, #002e55 0%, #075f96 62%, #1685be 100%)' }}>
      <Box sx={{ position: 'absolute', width: 520, height: 520, border: '90px solid rgba(255,255,255,.035)', borderRadius: '50%', top: -260, right: -120 }} />
      <Box sx={{ position: 'absolute', width: 380, height: 380, bgcolor: 'rgba(239,125,0,.09)', borderRadius: '50%', bottom: -250, left: -100 }} />
      <Container maxWidth="sm" sx={{ position: 'relative', zIndex: 1 }}>
        <Paper elevation={20} sx={{ overflow: 'hidden', border: 0, boxShadow: '0 28px 70px rgba(0,20,40,.34)' }}>
          <Box sx={{ px: { xs: 3, sm: 5 }, py: 3, bgcolor: '#f7fafc', borderBottom: '1px solid', borderColor: 'divider', display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 2 }}>
            <BrandMark />
            <Box sx={{ textAlign: 'right', display: { xs: 'none', sm: 'block' } }}>
              <Typography variant="caption" color="text.secondary" fontWeight={700}>PLATAFORMA OPERATIVA</Typography>
              <Typography variant="body2" color="primary.dark" fontWeight={850}>COTELNET</Typography>
            </Box>
          </Box>
          <Stack component="form" spacing={2.5} onSubmit={handleSubmit} sx={{ p: { xs: 3, sm: 5 } }}>
            <Box>
              <Typography variant="h4" color="primary.dark">Bienvenido</Typography>
              <Typography color="text.secondary" sx={{ mt: .5 }}>Ingresa tus credenciales para iniciar la jornada postal.</Typography>
            </Box>
            {error && <Alert severity="error">{error}</Alert>}
            <TextField label="Usuario" value={username} onChange={(e) => setUsername(e.target.value)} autoComplete="username" required autoFocus />
            <TextField label="Contraseña" type="password" value={password} onChange={(e) => setPassword(e.target.value)} autoComplete="current-password" required />
            <Button type="submit" size="large" variant="contained" disabled={loading} startIcon={!loading && <LockRoundedIcon />}>
              {loading ? <CircularProgress size={24} color="inherit" /> : 'Ingresar al sistema'}
            </Button>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: .75, color: 'text.secondary' }}>
              <VerifiedUserRoundedIcon sx={{ fontSize: 16, color: 'success.main' }} />
              <Typography variant="caption">Acceso seguro · Correos Panamá</Typography>
            </Box>
          </Stack>
        </Paper>
      </Container>
    </Box>
  )
}

