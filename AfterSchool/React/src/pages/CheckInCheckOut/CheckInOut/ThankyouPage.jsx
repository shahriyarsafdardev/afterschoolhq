import { Box, Button, Typography } from '@material-ui/core';
import { useParams } from 'react-router-dom';

import React, { useEffect } from 'react';
import useStyles from '../Common/styles';
import { useCheckInOut } from 'redux/hooks';
import utils from 'utils';

export default function ThankyouPage({ checkedStudents }) {
  const classes = useStyles();
  const { redirect } = utils;
  const { accountName } = useParams();
  const {
    checkedInStudents = localStorage.getItem('checkedInStudents'),
    checkedOutStudents = localStorage.getItem('checkedOutStudents')
  } = checkedStudents;

  const { clearSelectedStudents } = useCheckInOut();

  useEffect(() => {
    return () => {
      clearSelectedStudents();
    };
  }, []);

  return (
    <Box className={classes.thankyouPageWrapper}>
      <Typography className={classes.font18Bold}>Thank you!</Typography>
      <Box>
        {checkedInStudents && (
          <Typography className={classes.font16Light}>
            {checkedInStudents} have successfully been checked in.
          </Typography>
        )}
        {checkedOutStudents && (
          <Typography className={classes.font16Light}>
            {checkedOutStudents} have successfully been checked out.
          </Typography>
        )}
      </Box>

      <Button
        variant="outlined"
        color="primary"
        fullWidth
        onClick={() => {
          redirect(`/${accountName}`);
        }}
      >
        Close
      </Button>
    </Box>
  );
}
