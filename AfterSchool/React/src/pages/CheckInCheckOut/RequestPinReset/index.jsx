import React, { useEffect, useState } from 'react';
import useStyles from '../Common/styles';
import { useSnackbar } from 'notistack';
import { Box, Card, CircularProgress, Grid } from '@material-ui/core';
import { useSearchParams } from 'react-router-dom';
import PhoneNumberForm from '../Common/PhoneNumberForm';
import ResendLink from './ResendLink';
import { useCheckInOut } from 'redux/hooks';

function index() {
  const classes = useStyles();
  const { msgs, loading, resetPin } = useCheckInOut();
  const [activeStep, setActiveStep] = useState(0);
  const { enqueueSnackbar } = useSnackbar();
  const [searchParams, setSearchParams] = useSearchParams();
  const phone = decodeURI(searchParams.get('phone')) || '';
  const accountName = searchParams.get('accountName') || '';

  const handleSubmit = phoneNumber => {
    resetPin(phoneNumber, accountName).then(msgs => {
      if (msgs.length > 0) return;

      let query = `phone=${encodeURIComponent(phoneNumber)}`;
      if (accountName) query += `&accountName=${accountName}`;

      setSearchParams(query);
      setActiveStep(1);
    });
    localStorage.setItem('resetLinkSendAt', new Date().toISOString());
  };

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  useEffect(() => {
    if (phone) setActiveStep(1);
  }, [phone]);

  const innerSection = () => {
    if (!phone || phone === 'null')
      return (
        <PhoneNumberForm
          handleSubmit={handleSubmit}
          headerText="Reset Your PIN"
          subHeaderText="Please enter your cell phone number. "
          submitting={loading}
        />
      );
    if (activeStep == 1) return <ResendLink resendHandler={() => handleSubmit(phone)} />;

    return <CircularProgress className={classes.marginAuto} />;
  };
  return (
    <Grid container className={classes.wrapper}>
      <Card className={classes.innerContainer}>
        <Box className={classes.content}>{innerSection()}</Box>
      </Card>
    </Grid>
  );
}

export default index;
