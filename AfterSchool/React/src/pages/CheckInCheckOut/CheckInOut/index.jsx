import React, { useEffect, useState } from 'react';
import useStyles from '../Common/styles';
import { useSnackbar } from 'notistack';
import { Box, CircularProgress, Grid } from '@material-ui/core';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useCheckInOut } from 'redux/hooks';
import PhoneNumberForm from '../Common/PhoneNumberForm';
import PinInputForm from './PinInputForm';
import ChildList from './ChildList';
import EsignPage from './EsignPage';
import ThankyouPage from './ThankyouPage';

function index() {
  const classes = useStyles();
  const navigate = useNavigate();
  const { accountName } = useParams();
  const { enqueueSnackbar } = useSnackbar();
  const [searchParams, setSearchParams] = useSearchParams();
  const phone = searchParams.get('phone') || '';
  const userPin = searchParams.get('pin') || '';
  const success = searchParams.get('success') || false;
  const [userSignature, setUserSignature] = useState('');
  const [activeStep, setActiveStep] = useState(0);
  const [checkedStudents, setCheckedStudents] = useState({});

  const {
    msgs,
    loading,
    selectedStudents,
    loadChildListData,
    updateStudentCheckInOutInfo
  } = useCheckInOut();

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

    await updateStudentCheckInOutInfo(selectedStudents, phone, userPin, sign).then(res => {
      setActiveStep(3);
      setSearchParams('success=true');
    });
  };

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  useEffect(async () => {
    if (phone && userPin) {
      await loadChildListData(accountName, phone, userPin).then(msgs => {
        if (msgs.length > 0) navigate(`/reactapp/check-in-out/${accountName}`);
        setActiveStep(1);
      });
    }
  }, [phone, userPin]);

  const innerSection = () => {
    if (loading) return <CircularProgress className={classes.marginAuto} />;
    if (success) return <ThankyouPage checkedStudents={checkedStudents} />;
    if (!phone) return <PhoneNumberForm setSearchParams={setSearchParams} />;
    if (!userPin) return <PinInputForm setUserPin={handlePinInput} />;
    if (activeStep === 1) return <ChildList setActiveStep={setActiveStep} />;
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
