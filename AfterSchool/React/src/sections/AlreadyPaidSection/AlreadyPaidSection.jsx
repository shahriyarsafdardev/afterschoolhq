import React, { useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { Box, Button, Link, Typography } from '@material-ui/core';

import utils from 'utils';
import apiRoutes from 'routes';
import useStyles from './styles';
import { Loader } from 'components';
import { useThankYou } from 'redux/hooks';

function AlreadyPaidSection() {
  const { redirect } = utils;
  const classes = useStyles();
  const { registrationId } = useParams();
  const { makePaymentScreenPath, accountUrl } = apiRoutes;

  const {
    loading,
    data: { AmountPaid, ActivityName, Organization, ProviderEmail, RegistrationStatus },
    loadThankYouScreenData
  } = useThankYou();

  useEffect(() => {
    loadThankYouScreenData(registrationId);
  }, []);

  if (loading) return <Loader />;

  const redirectToPayment = () => redirect(makePaymentScreenPath(registrationId));

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
            href={`mailto:${ProviderEmail}?subject=Inquiry About ${ActivityName}`}
            data-testid={'Email Provider'}
          >
            Email Provider
          </Button>
          <Button
            color="primary"
            className={classes.viewAccountButton}
            variant="contained"
            type="submit"
            onClick={redirectToPayment}
            data-testid={'Continue to Payment'}
          >
            Continue to Payment
          </Button>
        </Box>
        <Link
          href={accountUrl}
          className={classes.link}
          variant="body1"
          data-testid={'Return to Account'}
        >
          Return to Account
        </Link>
      </Box>
    </Box>
  );
}

export default AlreadyPaidSection;
