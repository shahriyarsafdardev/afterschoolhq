import React from 'react';
import { Box, Typography } from '@material-ui/core';
import useStyles from '../Common/styles';

export default function ThankyouScreen() {
  const classes = useStyles();

  return (
    <Box className={classes.pinSetupForm}>
      <Typography className={classes.headerText}>Thank you!</Typography>
      <Typography className={classes.subHeaderText}>
        Your PIN code has successfully been set up.
      </Typography>
    </Box>
  );
}
