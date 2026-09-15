import { createTheme } from '@mui/material/styles'

const theme = createTheme({
  palette: {
    primary: { main: '#003b70', dark: '#00284d', light: '#2f6696' },
    secondary: { main: '#d6a72c' },
    background: { default: '#f3f6f9', paper: '#ffffff' },
  },
  typography: {
    fontFamily: 'Inter, Segoe UI, Arial, sans-serif',
    h4: { fontWeight: 700 },
    h6: { fontWeight: 700 },
  },
  shape: { borderRadius: 10 },
  components: {
    MuiButton: { styleOverrides: { root: { textTransform: 'none', fontWeight: 700 } } },
  },
})

export default theme

