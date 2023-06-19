import React from 'react';
import { Box, Typography } from '@material-ui/core';

import useStyles from './styles';

export default function TextBlock({ question }) {
  const classes = useStyles();
  const { Description } = question;
  return (
    <Box style={{ maxWidth: '675px', margin: 'auto', marginBottom: 15 }}>
      <Box style={{ margin: 'auto' }}>
        <Typography className={classes.fieldLabel}>{Description}</Typography>
      </Box>
    </Box>
  );
}
