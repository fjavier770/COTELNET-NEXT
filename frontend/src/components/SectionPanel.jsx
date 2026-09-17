import { Box, Paper, Typography } from '@mui/material'

export default function SectionPanel({ title, icon, action, children, sx }) {
  return (
    <Paper elevation={0} sx={{ overflow: 'hidden', ...sx }}>
      <Box sx={{
        minHeight: 40,
        px: 2,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: 1,
        color: '#fff',
        background: 'linear-gradient(90deg, #075f96 0%, #167db8 100%)',
      }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          {icon}
          <Typography variant="subtitle2" sx={{ color: 'inherit', fontWeight: 800, letterSpacing: '.35px' }}>{title}</Typography>
        </Box>
        {action}
      </Box>
      <Box sx={{ p: { xs: 2, md: 2.5 } }}>{children}</Box>
    </Paper>
  )
}
