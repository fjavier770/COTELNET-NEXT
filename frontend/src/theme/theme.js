import { createTheme } from '@mui/material/styles'

const theme = createTheme({
  palette: {
    primary: { main: '#075f96', dark: '#003b70', light: '#3f8fbd' },
    secondary: { main: '#ef7d00', dark: '#c65f00', light: '#ffad4a', contrastText: '#fff' },
    success: { main: '#16835b' },
    background: { default: '#edf1f4', paper: '#ffffff' },
    text: { primary: '#183247', secondary: '#627482' },
    divider: '#d7e0e6',
  },
  typography: {
    fontFamily: 'Inter, Segoe UI, Arial, sans-serif',
    h3: { fontWeight: 850, letterSpacing: '-.04em' },
    h4: { fontWeight: 800, letterSpacing: '-.025em' },
    h5: { fontWeight: 800 },
    h6: { fontWeight: 800 },
    button: { fontWeight: 800 },
  },
  shape: { borderRadius: 7 },
  components: {
    MuiCssBaseline: {
      styleOverrides: {
        body: { scrollbarColor: '#9db3c2 #edf1f4' },
        '::selection': { backgroundColor: '#b8ddf2', color: '#003b70' },
      },
    },
    MuiButton: {
      defaultProps: { disableElevation: true },
      styleOverrides: { root: { textTransform: 'none', fontWeight: 800, borderRadius: 6, minHeight: 38 } },
    },
    MuiPaper: { styleOverrides: { root: { backgroundImage: 'none', border: '1px solid #d7e0e6' } } },
    MuiCard: { styleOverrides: { root: { border: '1px solid #d7e0e6', boxShadow: '0 8px 24px rgba(0,59,112,.06)' } } },
    MuiTextField: { defaultProps: { size: 'small' } },
    MuiFormControl: { defaultProps: { size: 'small' } },
    MuiInputLabel: { styleOverrides: { root: { fontWeight: 650 } } },
    MuiOutlinedInput: { styleOverrides: { root: { backgroundColor: '#fff' } } },
    MuiTableHead: {
      styleOverrides: { root: { backgroundColor: '#e7f2f8', '& .MuiTableCell-root': { color: '#003b70', fontWeight: 800 } } },
    },
    MuiTableCell: { styleOverrides: { root: { borderColor: '#dce4e9' } } },
    MuiDialogTitle: { styleOverrides: { root: { color: '#fff', background: 'linear-gradient(90deg,#075f96,#167db8)', fontWeight: 800 } } },
    MuiChip: { styleOverrides: { root: { fontWeight: 750 } } },
  },
})

export default theme

