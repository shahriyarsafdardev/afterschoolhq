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

function index() {
  const classes = useStyles();
  const navigate = useNavigate();
  const { accountName } = useParams();
  const { enqueueSnackbar } = useSnackbar();
  const [searchParams, setSearchParams] = useSearchParams();
  const phone = searchParams.get('phone') || '';
  const userPin = searchParams.get('pin') || '';
  const [userSignature, setUserSignature] = useState('');
  const [activeStep, setActiveStep] = useState(0);

  const {
    msgs,
    loadChildListData,
    childData: { StudentsInfo = [] },
    loading,
    selectedStudents,
    updateStudentCheckInOutInfo
  } = useCheckInOut();

  const handlePinInput = pin => {
    setSearchParams(`phone=${phone}&pin=${pin}`);
  };

  const setSignature = async sign => {
    setUserSignature(sign);

    await updateStudentCheckInOutInfo(selectedStudents, phone, userPin).then(res => {
      setActiveStep(2);
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
