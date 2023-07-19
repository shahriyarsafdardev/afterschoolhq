import { Box, Button, Typography } from '@material-ui/core';
import useStyles from '../Common/styles';
import utils from 'utils';
import React from 'react';

export default function Thankyou() {
  const classes = useStyles();
  const { navigateTo } = utils;
  const accountName = localStorage.getItem('accountName');

  return (
    <Box className={classes.thankyouPageWrapper}>
      <Typography className={classes.font18Bold}>Success! Your PIN has been reset.</Typography>

      {accountName && (
        <Button
          variant="contained"
          color="primary"
          fullWidth
          onClick={() => {
            navigateTo(`/reactapp/check-in-out/${accountName}`);
          }}
        >
          Return to Check In/Out
        </Button>
      )}
    </Box>
  );
}
