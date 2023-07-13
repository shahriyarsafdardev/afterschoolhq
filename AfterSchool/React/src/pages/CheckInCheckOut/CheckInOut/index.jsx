import React, { useEffect, useState } from 'react';
import useStyles from '../Common/styles';
import { useSnackbar } from 'notistack';
import { Box, Card, CircularProgress, Grid } from '@material-ui/core';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useCheckInOut } from 'redux/hooks';
import { isEmpty } from 'lodash';
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
  const [selectedChilds, setSelectedChilds] = useState({});

  const {
    msgs,
    loadChildListData,
    childData: { AccountName, StudentsInfo = [] },
    loading
  } = useCheckInOut();

  const handlePinInput = pin => {
    setSearchParams(`phone=${phone}&pin=${pin}`);
  };

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  useEffect(async () => {
    if (phone && userPin) {
      await loadChildListData(accountName, phone, userPin).then(msgs => {
        if (msgs.length > 0) navigate(`/reactapp/check-in-out/${accountName}`);
      });
    }
  }, [phone, userPin]);

  const innerSection = () => {
    if (loading) return <CircularProgress className={classes.marginAuto} />;
    if (!phone) return <PhoneNumberForm setSearchParams={setSearchParams} />;
    if (!userPin) return <PinInputForm setUserPin={handlePinInput} />;
    if (phone && userPin && !isEmpty(StudentsInfo) && isEmpty(selectedChilds))
      return (
        <ChildList
          setSelectedChilds={setSelectedChilds}
          StudentsInfo={StudentsInfo}
          AccountName={AccountName}
        />
      );
    if (!isEmpty(selectedChilds)) return <EsignPage />;

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
