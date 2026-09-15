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
    <Box sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center', background: 'linear-gradient(135deg, #00284d, #0c5b93)' }}>
      <Container maxWidth="xs">
        <Paper elevation={12} sx={{ p: { xs: 3, sm: 5 } }}>
          <Stack component="form" spacing={3} onSubmit={handleSubmit}>
            <Box>
              <Typography variant="overline" color="secondary.main" fontWeight={800}>Correos Panamá</Typography>
              <Typography variant="h4" color="primary">COTELNET</Typography>
              <Typography color="text.secondary">Acceso al sistema postal</Typography>
            </Box>
            {error && <Alert severity="error">{error}</Alert>}
            <TextField label="Usuario" value={username} onChange={(e) => setUsername(e.target.value)} autoComplete="username" required autoFocus />
            <TextField label="Contraseña" type="password" value={password} onChange={(e) => setPassword(e.target.value)} autoComplete="current-password" required />
            <Button type="submit" size="large" variant="contained" disabled={loading}>
              {loading ? <CircularProgress size={24} color="inherit" /> : 'Ingresar'}
            </Button>
          </Stack>
        </Paper>
      </Container>
    </Box>
  )
}

