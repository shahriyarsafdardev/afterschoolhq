import { createTheme } from '@material-ui/core/styles';
import typography from './typography';
import palette from './palette';
import overrides from './overrides';

export default createTheme({
  palette,
  breakpoints: {
    values: {
      xs: 0,
      sm: 600,
      md: 900,
      lg: 1200,
      xl: 1536,
    },
  },
  typography,
  overrides,
});
