import React, { useEffect, useState } from 'react';
import { Box, Button, Typography, Link } from '@material-ui/core';
import useStyles from '../Common/styles';

const RESEND_LIMIT = 30;

function ResendLink({
  resendHandler,
  headerText = 'We’ve sent you a text with a link to reset your PIN.',
  subHeaderText = ''
}) {
  const classes = useStyles();
  const [timeDifference, setTimeDifference] = useState(0);
  const resetLinkSendAt = localStorage.getItem('resetLinkSendAt');

  useEffect(() => {
    const savedDateTime = localStorage.getItem('resetLinkSendAt');
    const savedTime = new Date(savedDateTime);

    const updateTimeDifference = () => {
      const currentTime = new Date();
      const newTimeDifference = Math.round((currentTime - savedTime) / 1000);
      const timeLeft = RESEND_LIMIT - newTimeDifference;

      if (timeLeft >= 0) setTimeDifference(timeLeft);
      if (timeLeft <= 0) clearInterval(interval); // Cleanup on component unmount
    };

    updateTimeDifference(); // Initial update

    const interval = setInterval(updateTimeDifference, 1000); // Update every second

    return () => {
      clearInterval(interval); // Cleanup on component unmount
    };
  }, [resetLinkSendAt]);

  const handleSubmit = () => {
    resendHandler();
  };

  const canResend = timeDifference === 0;

  return (
    <Box className={classes.phoneForm}>
      <Typography className={classes.headerText}>{headerText}</Typography>
      {subHeaderText && <Typography className={classes.subHeaderText}>{subHeaderText}</Typography>}

      <Button
        onClick={handleSubmit}
        className={classes.buttonNext}
        variant="contained"
        color="primary"
        disabled={!canResend}
      >
        Resend Text
      </Button>

      {!canResend && (
        <Typography className={classes.link} variant="body1">
          Resend available again in {timeDifference} seconds
        </Typography>
      )}

      <Link
        href="https://afterschoolhq.zendesk.com/hc/en-us/requests/new"
        target="_blank"
        className={classes.link}
        variant="body1"
        data-testid={''}
      >
        Contact Support
      </Link>
    </Box>
  );
}

export default ResendLink;
