import React, { useEffect, useState } from 'react';
import { Box, Typography } from '@material-ui/core';
import useStyles from './styles';
import PinSetupForm from './PinSetupForm';
import AuthorizedPersonListSetup from './AuthorizedPersonListSetup';
import { usePostRegistration } from 'redux/hooks';
import { useNavigate, useParams } from 'react-router-dom';
import { Loader } from 'components';
import apiRoutes from 'routes';

export default function PostRegistrationSetup() {
  const classes = useStyles();
  const navigate = useNavigate();
  const { registrationUid } = useParams();
  const { thankyouUrl } = apiRoutes;
  const [activeStep, setActiveStep] = useState(0);

  const {
    loading,
    postRegistration,
    data: { additionalContactDto = [], setupDto, activityDto }
  } = usePostRegistration();

  const { RegistrantType, StudentFirstName } = activityDto || {};
  const { IsPhoneSetup, IsPinSetup } = setupDto || {};

  useEffect(() => {
    getRegistrationData();
  }, []);

  const getRegistrationData = () => {
    postRegistration(registrationUid);
  };

  useEffect(() => {
    if (!IsPhoneSetup || !IsPinSetup) {
      setActiveStep(0);
    } else if (RegistrantType === 'Parent') {
      getRegistrationData();
      setActiveStep(1);
    } else {
      setActiveStep(2);
    }
  }, [IsPhoneSetup, IsPinSetup]);

  const renderActiveStep = () => {
    if (activeStep === 0)
      return (
        <PinSetupForm
          setActiveStep={setActiveStep}
          setupDto={setupDto}
          RegistrantType={RegistrantType}
        />
      );
    else if (activeStep === 1)
      return (
        <AuthorizedPersonListSetup
          authorizedPersons={additionalContactDto}
          setActiveStep={setActiveStep}
        />
      );
    else {
      navigate(thankyouUrl(registrationUid));
      return null;
    }
  };

  const selectHeading = () => {
    if (activeStep === 0) {
      return [
        'Pin Setup',
        'You have registered for an activity that will require you to check in and check out your child each day.',
        'Please select four numbers to use for your PIN code during check in and check out.'
      ];
    }
    return [
      'Invite Authorized Check In/Out Person',
      `Do you want anyone else to have permission to pick up or drop off ${StudentFirstName}?`,
      'They will be notified and prompted to create their own PIN code for checkin and checkout.'
    ];
  };

  if (loading) return <Loader />;

  return (
    <Box className={classes.marginWrapper}>
      <Box className={classes.innerWrapper}>
        <Typography className={classes.sectionName} variant="h6" color="primary">
          {selectHeading()[0]}
        </Typography>
        <Typography variant="h6">{selectHeading()[1]}</Typography>
        <Typography>{selectHeading()[2]}</Typography>
        {renderActiveStep()}
      </Box>
    </Box>
  );
}
