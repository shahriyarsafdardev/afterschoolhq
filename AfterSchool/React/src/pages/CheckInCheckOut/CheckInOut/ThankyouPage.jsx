import { Box, Button, Typography } from '@material-ui/core';
import React, { useEffect } from 'react';
import useStyles from '../Common/styles';

export default function ThankyouPage({ checkedStudents }) {
  const classes = useStyles();
  const {
    checkedInStudents = localStorage.getItem('checkedInStudents'),
    checkedOutStudents = localStorage.getItem('checkedOutStudents')
  } = checkedStudents;

  return (
    <Box className={classes.thankyouPageWrapper}>
      <Typography className={classes.font18Bold}>Thank you!</Typography>
      {checkedInStudents && (
        <Typography className={classes.font18Light}>
          {checkedInStudents} have successfully been checked in.
        </Typography>
      )}
      {checkedOutStudents && (
        <Typography className={classes.font18Light}>
          {checkedOutStudents} have successfully been checked out.
        </Typography>
      )}
      <Button
        variant="outlined"
        color="primary"
        fullWidth
        onClick={() => {
          window.close();
        }}
      >
        Close
      </Button>
    </Box>
  );
}
