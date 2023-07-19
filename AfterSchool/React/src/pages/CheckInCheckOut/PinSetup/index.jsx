import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { Box, Card, CircularProgress, Grid } from '@material-ui/core';
import React, { useEffect, useState } from 'react';
import { useSnackbar } from 'notistack';
import { isEmpty } from 'lodash';

import PhoneNumberForm from '../Common/PhoneNumberForm';
import ConfirmPinForm from '../Common/ConfirmPinForm';
import ThankyouScreen from './ThankyouScreen';
import { useCheckInOut } from 'redux/hooks';
import PinSetupForm from './PinSetupForm';
import useStyles from '../Common/styles';

function index() {
  const classes = useStyles();
  const [userPin, setUserPin] = useState();
  const [isSetupCompleted, setIsSetupCompleted] = useState(false);
  const navigate = useNavigate();
  const { registrationUid } = useParams();
  const { enqueueSnackbar } = useSnackbar();
  const [searchParams, setSearchParams] = useSearchParams();
  const phone = searchParams.get('phone') || '';

  const {
    msgs,
    loadPinData,
    updatePinData,
    pinSetupData: { currentUserInfo = {} },
    loading
  } = useCheckInOut();

  const { IsPinSetup } = currentUserInfo;

  const handleSubmitPinCode = pin => {
    updatePinData(registrationUid, phone, pin);
    setIsSetupCompleted(true);
  };

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  useEffect(async () => {
    if (phone) {
      await loadPinData(registrationUid, phone).then(msgs => {
        if (msgs.length > 0) navigate(`/reactapp/pin-setup/${registrationUid}`);
      });
    }
  }, [phone]);

  const handleSubmit = phoneNumber => {
    setSearchParams(`phone=${phoneNumber}`);
  };

  const innerSection = () => {
    if (loading) return <CircularProgress className={classes.marginAuto} />;
    if (isSetupCompleted || IsPinSetup) return <ThankyouScreen />;
    if (!phone) return <PhoneNumberForm handleSubmit={handleSubmit} />;
    if (!userPin && !isEmpty(currentUserInfo))
      return <PinSetupForm setUserPin={setUserPin} currentUserInfo={currentUserInfo} />;
    if (!isEmpty(currentUserInfo))
      return <ConfirmPinForm userPin={userPin} handleSubmitPinCode={handleSubmitPinCode} />;

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
