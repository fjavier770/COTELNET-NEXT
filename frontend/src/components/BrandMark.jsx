import LocalPostOfficeRoundedIcon from '@mui/icons-material/LocalPostOfficeRounded'
import { Box, Typography } from '@mui/material'

export default function BrandMark({ compact = false, light = false }) {
  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: compact ? 1 : 1.25, minWidth: 0 }}>
      <Box sx={{
        width: compact ? 34 : 42,
        height: compact ? 34 : 42,
        display: 'grid',
        placeItems: 'center',
        bgcolor: 'secondary.main',
        color: '#fff',
        borderRadius: 1.25,
        boxShadow: '0 5px 14px rgba(239,125,0,.28)',
        flexShrink: 0,
      }}>
        <LocalPostOfficeRoundedIcon fontSize={compact ? 'small' : 'medium'} />
      </Box>
      <Box sx={{ lineHeight: 1, minWidth: 0 }}>
        <Typography sx={{ color: light ? '#fff' : 'primary.main', fontWeight: 900, fontSize: compact ? 17 : 21, letterSpacing: '-.5px', lineHeight: 1 }}>
          CORREOS
        </Typography>
        <Typography sx={{ color: 'secondary.main', fontWeight: 900, fontSize: compact ? 11 : 13, letterSpacing: 1.5, lineHeight: 1.25 }}>
          PANAMÁ
        </Typography>
      </Box>
    </Box>
  )
}
