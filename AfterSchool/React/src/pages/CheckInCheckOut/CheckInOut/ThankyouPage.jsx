import { Box, Button, Typography } from '@material-ui/core';
import React from 'react';
import useStyles from '../Common/styles';

export default function ThankyouPage({ checkedStudents }) {
  const classes = useStyles();
  const { checkedInStudents, checkedOutStudents } = checkedStudents;

  return (
    <Box className={classes.esignPage}>
      <Typography className={classes.headerText}>Thank you!</Typography>
      {checkedInStudents && (
        <Typography className={classes.subHeaderText}>
          {checkedInStudents} have successfully been checked in.
        </Typography>
      )}
      {checkedOutStudents && (
        <Typography className={classes.subHeaderText}>
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
