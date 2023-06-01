import { Box, FormHelperText } from '@material-ui/core';
import React from 'react';

import useStyles from './styles';

function ErrorBand({ error }) {
  const classes = useStyles();
  return (
    <Box className={classes.flexCenter}>
      <FormHelperText className={classes.maxWidth} error>
        {error}
      </FormHelperText>
    </Box>
  );
}

export default ErrorBand;
