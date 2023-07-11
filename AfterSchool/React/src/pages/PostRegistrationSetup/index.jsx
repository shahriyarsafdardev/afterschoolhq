import React, { useEffect, useState } from 'react';
import { Box, Typography } from '@material-ui/core';
import useStyles from './styles';
import PinSetupForm from './PinSetupForm';
import PeopleListSetup from './PeopleListSetup';
import { usePostRegistration } from 'redux/hooks';
import { useNavigate, useParams } from 'react-router-dom';
import { Loader } from 'components';
import utils from 'utils';
import apiRoutes from 'routes';
import { applyModes } from 'constants';

export default function PostRegistrationSetup() {
  const classes = useStyles();
  const navigate = useNavigate();
  const { registrationUid } = useParams();
  const { redirect } = utils;
  const { paymentScreenPath, thankyouUrl } = apiRoutes;
  const [activeStep, setActiveStep] = useState(0);

  const {
    loading,
    postRegistration,
    data: { additionalContactDto = [], setupDto, activityDto }
  } = usePostRegistration();

  const { RegistrantType, IsFree, ApplyMode } = activityDto || {};
  const { IsPhoneSetup, IsPinSetup } = setupDto || {};

  useEffect(() => {
    postRegistration(registrationUid);
  }, []);

  useEffect(() => {
    if (!IsPhoneSetup || !IsPinSetup) {
      setActiveStep(0);
    } else if (RegistrantType === 'Parent') {
      setActiveStep(1);
    } else {
      setActiveStep(2);
    }
  }, [IsPhoneSetup, IsPinSetup]);

  const renderActiveStep = () => {
    if (activeStep === 0) return <PinSetupForm setActiveStep={setActiveStep} setupDto={setupDto} />;
    else if (activeStep === 1)
      return <PeopleListSetup listOfPeople={additionalContactDto} setActiveStep={setActiveStep} />;
    else {
      if (IsFree) navigate(thankyouUrl(registrationUid));
      else {
        ApplyMode === applyModes.Apply
          ? navigate(thankyouUrl(registrationUid))
          : redirect(paymentScreenPath(registrationUid));
      }
      return null;
    }
  };

  const selectHeading = () => {
    if (activeStep === 0) {
      return [
        'You have registered for an activity that will require you to check in and check out your child each day.',
        'Please select four numbers to use for your PIN code during check in and check out.'
      ];
    }
    return [
      'Do you want anyone else to have permission to pick up or drop off Jojo?',
      'They will be notified and prompted to create their own PIN code for checkin and checkout.'
    ];
  };

  if (loading) return <Loader />;

  return (
    <Box style={{ marginTop: '25px' }}>
      <Box
        style={{
          maxWidth: '600px',
          margin: 'auto',
          textAlign: 'center',
          display: 'flex',
          flexDirection: 'column',
          gap: '20px'
        }}
      >
        <Typography className={classes.sectionName} variant="h6" color="primary">
          Pin Setup
        </Typography>
        <Typography variant="h6">{selectHeading()[0]}</Typography>
        <Typography>{selectHeading()[1]}</Typography>
        {renderActiveStep()}
      </Box>
    </Box>
  );
}
