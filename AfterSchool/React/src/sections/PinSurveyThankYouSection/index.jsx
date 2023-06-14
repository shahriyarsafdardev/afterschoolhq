import React from 'react';
import { Box, Button, Typography } from '@material-ui/core';

import utils from 'utils';
import useStyles from './styles';

function PinSurveyThankYouSection() {
  const classes = useStyles();
  const { navigateTo } = utils;

  return (
    <Box className={classes.wrapper}>
      <Box className={classes.thankYouWrapper}>
        <Typography variant="h4">Thank You!</Typography>
        <Typography variant="body1">Your response have been submitted.</Typography>
        <Box>
          <Button
            color="primary"
            variant="outlined"
            onClick={() => navigateTo('/login')}
            data-testid={'Exit'}
          >
            Exit
          </Button>
        </Box>
      </Box>
    </Box>
  );
}

export default PinSurveyThankYouSection;
