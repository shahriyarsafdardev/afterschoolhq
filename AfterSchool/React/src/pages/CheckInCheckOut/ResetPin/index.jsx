import { Box, Card, CircularProgress, Grid } from '@material-ui/core';
import React, { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import ConfirmPinForm from '../Common/ConfirmPinForm';
import { useCheckInOut } from 'redux/hooks';
import PinSetupForm from './PinSetupForm';
import useStyles from '../Common/styles';
import Thankyou from './Thankyou';

function index() {
  const classes = useStyles();
  const [userPin, setUserPin] = useState();
  const [searchParams, setSearchParams] = useSearchParams();
  const token = searchParams.get('token') || '';
  const success = searchParams.get('success') || '';
  const accountName = searchParams.get('accountName') || '';

  const [activeStep, setActiveStep] = useState(1);

  const { loading, submitResetPin } = useCheckInOut();

  useEffect(() => {
    if (userPin) setActiveStep(2);
  }, [userPin]);

  useEffect(() => {
    const sessionCompleted = sessionStorage.getItem('isSessionCompleted');
    if (sessionCompleted) {
      setSearchParams('success=true');
    }
  }, [success]);

  const handleSubmit = async pin => {
    await submitResetPin(token, pin).then(msgs => {
      if (!msgs.length > 0) {
        sessionStorage.setItem('accountName', accountName);
        sessionStorage.setItem('isSessionCompleted', true);
        setActiveStep(3);
        setSearchParams('success=true');
      }
    });
  };

  const innerSection = () => {
    if (loading) return <CircularProgress className={classes.marginAuto} />;
    if (success) return <Thankyou />;
    if (activeStep === 1) return <PinSetupForm setUserPin={setUserPin} />;
    if (activeStep === 2)
      return <ConfirmPinForm userPin={userPin} handleSubmitPinCode={handleSubmit} />;

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
