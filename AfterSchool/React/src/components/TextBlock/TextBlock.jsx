import React from 'react';
import { Box, Typography } from '@material-ui/core';

import useStyles from './styles';

export default function TextBlock({ question }) {
  const classes = useStyles();
  const { Description } = question;
  return (
    <Box className={classes.outerWrapper}>
      <Box className={classes.innerWrapper}>
        <Typography className={classes.fieldLabel}>{Description}</Typography>
      </Box>
    </Box>
  );
}
