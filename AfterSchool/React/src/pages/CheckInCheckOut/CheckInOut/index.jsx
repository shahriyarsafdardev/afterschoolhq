import React, { useEffect, useState } from 'react';
import useStyles from '../Common/styles';
import { Box, CircularProgress, Grid } from '@material-ui/core';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useCheckInOut } from 'redux/hooks';
import PhoneNumberForm from '../Common/PhoneNumberForm';
import PinInputForm from './PinInputForm';
import ChildList from './ChildList';
import EsignPage from './EsignPage';
import ThankyouPage from './ThankyouPage';
import { getPhoneFromParams, setPhoneNumber } from '../Common';

function index() {
  const classes = useStyles();
  const navigate = useNavigate();
  const { accountName } = useParams();
  const [searchParams, setSearchParams] = useSearchParams();
  const phone = getPhoneFromParams(searchParams);
  const userPin = searchParams.get('pin') || '';
  const success = searchParams.get('success') || false;
  const [userSignature, setUserSignature] = useState('');
  const [activeStep, setActiveStep] = useState(0);
  const [checkedStudents, setCheckedStudents] = useState({});

  const {
    loading,
    selectedStudents,
    loadChildListData,
    updateStudentCheckInOutInfo
  } = useCheckInOut();

  useEffect(() => {
    const sessionCompleted = sessionStorage.getItem('isSessionCompleted');
    if (!success && !phone && !userPin) {
      sessionStorage.removeItem('isSessionCompleted');
    } else if (sessionCompleted) {
      setSearchParams('success=true');
    }
  }, [phone, userPin]);

  const handlePinInput = pin => {
    setSearchParams(`phone=${phone}&pin=${pin}`);
  };

  const setSignature = async (sign, checkedInStudents, checkedOutStudents) => {
    setUserSignature(sign);
    setCheckedStudents({
      checkedInStudents: checkedInStudents,
      checkedOutStudents: checkedOutStudents
    });

    localStorage.setItem('checkedInStudents', checkedInStudents);
    localStorage.setItem('checkedOutStudents', checkedOutStudents);

    await updateStudentCheckInOutInfo(selectedStudents, phone, userPin, sign).then(messages => {
      if (messages.length > 0) return;
      setActiveStep(3);
      setSearchParams('success=true');
      sessionStorage.setItem('isSessionCompleted', true);
    });
  };

  const navigateToRoot = () => navigate(`/reactapp/check-in-out/${accountName}`);

  useEffect(async () => {
    if (phone && userPin) {
      await loadChildListData(accountName, phone, userPin).then(msgs => {
        if (msgs.length > 0) navigateToRoot();
        setActiveStep(1);
      });
    }
  }, [phone, userPin]);

  const handleSubmit = phoneNumber => {
    setPhoneNumber(phoneNumber, setSearchParams);
  };

  const innerSection = () => {
    if (loading) return <CircularProgress className={classes.marginAuto} />;
    if (success) return <ThankyouPage checkedStudents={checkedStudents} />;
    if (!phone) return <PhoneNumberForm handleSubmit={handleSubmit} />;
    if (!userPin)
      return (
        <PinInputForm
          setUserPin={handlePinInput}
          resetPinLink={`/reactapp/check-in-out/request-pin-reset/?accountName=${accountName}`}
        />
      );
    if (activeStep === 1)
      return <ChildList setActiveStep={setActiveStep} navigateToRoot={navigateToRoot} />;
    if (activeStep === 2)
      return <EsignPage setSignature={setSignature} setActiveStep={setActiveStep} />;

    return <CircularProgress className={classes.marginAuto} />;
  };

  return (
    <Grid container className={classes.wrapper}>
      <Box className={classes.innerContainer}>
        <Box className={classes.content}>{innerSection()}</Box>
      </Box>
    </Grid>
  );
}

export default index;
