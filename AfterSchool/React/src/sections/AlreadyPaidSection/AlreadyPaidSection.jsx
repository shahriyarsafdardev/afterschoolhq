import React, { useEffect } from 'react';
import { Box, Button, Link, Typography } from '@material-ui/core';
import { useParams } from 'react-router-dom';

import utils from 'utils';
import apiRoutes from 'routes';
import useStyles from './styles';
import { useThankYou } from 'redux/hooks';

function AlreadyPaidSection() {
  const classes = useStyles();
  const { navigateTo } = utils;
  const { paymentScreenPath } = apiRoutes;
  const { registrationId } = useParams();

  const {
    data: {
      AmountPaid,
      ActivityName,
      Organization,
      ProviderEmail,
      RegistrationStatus,
      RegisterForNewChildUrl
    },
    loadThankYouScreenData
  } = useThankYou();

  useEffect(() => {
    loadThankYouScreenData(registrationId);
  }, []);

  return (
    <Box className={classes.wrapper}>
      <Box className={classes.paymentWrapper}>
        <Typography variant="h4">Payment Already Submitted</Typography>
        <Typography variant="h6" className={classes.fontWeight500}>
          You have already paid <b>${AmountPaid}</b> to <b>{Organization}</b> for{' '}
          <b>{ActivityName}</b> and have a current status of <b>{RegistrationStatus}</b>. Are you
          sure you want to continue to payment?
        </Typography>
        <Typography variant="h6" className={classes.fontWeight500}>
          If you have any questions or believe this is an error, please contact the provider.
        </Typography>
        <Box>
          <Button
            color="primary"
            variant="outlined"
            className={classes.anotherActivityButton}
            rel="noopener noreferrer"
            href={`mailto:${ProviderEmail}`}
          >
            Email Provider
          </Button>
          <Button
            color="primary"
            className={classes.viewAccountButton}
            variant="contained"
            type="submit"
            onClick={() => navigateTo(paymentScreenPath(registrationId))}
          >
            Continue to Payment
          </Button>
        </Box>

        <Link href={RegisterForNewChildUrl} className={classes.link} variant="body1">
          Return to Account
        </Link>
      </Box>
    </Box>
  );
}

export default AlreadyPaidSection;
