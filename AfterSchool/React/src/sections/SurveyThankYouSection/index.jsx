import React from 'react';
import { useParams } from 'react-router-dom';
import { Box, Button, Typography } from '@material-ui/core';

import utils from 'utils';
import apiRoutes from 'routes';
import useStyles from './styles';

function SurveyThankYouSection() {
  const classes = useStyles();
  const { navigateTo } = utils;
  const { surveyUrl } = apiRoutes;
  const { formUid } = useParams();

  return (
    <Box className={classes.wrapper}>
      <Box className={classes.thankYouWrapper}>
        <Typography variant="h4">Thank You!</Typography>
        <Typography variant="body1">Do you want to submit a new survey form?</Typography>
        <Box>
          <Button
            color="primary"
            variant="outlined"
            onClick={() => navigateTo('/login')}
            data-testid={'Exit'}
          >
            Exit
          </Button>
          <Button
            color="primary"
            className={classes.submitNewSurveyButton}
            variant="contained"
            type="submit"
            onClick={() => navigateTo(surveyUrl(formUid))}
            data-testid={'Submit a new survey'}
          >
            Submit a new survey
          </Button>
        </Box>
      </Box>
    </Box>
  );
}

export default SurveyThankYouSection;
