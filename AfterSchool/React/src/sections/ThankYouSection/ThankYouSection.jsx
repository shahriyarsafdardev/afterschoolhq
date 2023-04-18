import { useSnackbar } from 'notistack';
import { useParams } from 'react-router-dom';
import React, { useState, useEffect } from 'react';
import { Box, Button, Link, Typography, Icon } from '@material-ui/core';

import utils from 'utils';
import apiRoutes from 'routes';
import useStyles from './styles';
import { useThankYou } from 'redux/hooks';
import ShareButton from 'components/ShareButton';
import CopyUrlIcon from 'assets/icon-ios7-world-outline.svg';

function ThankYouSection({ volunteerRegistration = false }) {
  const classes = useStyles();
  const { navigateTo } = utils;
  const { accountUrl } = apiRoutes;
  const { registrationId } = useParams();
  const { enqueueSnackbar } = useSnackbar();
  const [copySuccess, setCopySuccess] = useState('Copy URL');

  const {
    data: {
      isParent,
      ActivityUrl,
      ActivityName,
      RegistrantFirstName,
      FindAnotherActivityUrl,
      RegisterForNewChildUrl
    },
    loadThankYouScreenData
  } = useThankYou();

  useEffect(() => {
    loadThankYouScreenData(registrationId, volunteerRegistration);
  }, []);

  const copyToClipBoard = async copyMe => {
    try {
      await navigator.clipboard.writeText(copyMe);
      setCopySuccess('Copied!');
      enqueueSnackbar('URL has been Copied!', { variant: 'success' });
    } catch (err) {
      setCopySuccess('Failed to copy!');
      enqueueSnackbar('Failed to copy!', { variant: 'error' });
    }
  };

  const CopyButton = () => (
    <Box
      onClick={() => copyToClipBoard(ActivityUrl)}
      className={classes.shareBox}
      title="Copy URL"
      data-testid={'Copy URL'}
    >
      <Icon className={classes.icon}>
        <img className={classes.icon} src={CopyUrlIcon} />
      </Icon>
      <Typography>{copySuccess}</Typography>
    </Box>
  );

  const ShareButtons = () =>
    ['Facebook', 'Twitter'].map(platformName => (
      <ShareButton
        key={platformName}
        platformName={platformName}
        activityName={ActivityName}
        activityUrl={ActivityUrl}
      />
    ));

  return (
    <Box className={classes.wrapper}>
      <Box className={classes.thankYouWrapper}>
        <Typography variant="h4">Thank You!</Typography>
        <Typography variant="body1">
          <b>{RegistrantFirstName}</b> has been registered for <b>{ActivityName}</b>
        </Typography>
        <Box>
          {volunteerRegistration ? (
            <Button
              color="primary"
              className={classes.findOpportunityButton}
              variant="contained"
              type="submit"
              onClick={() => navigateTo(FindAnotherActivityUrl)}
              data-testid={'Find Another Oppurtunity'}
            >
              Find Another Oppurtunity
            </Button>
          ) : (
            <>
              <Button
                color="primary"
                variant="outlined"
                className={classes.anotherActivityButton}
                onClick={() => navigateTo(FindAnotherActivityUrl)}
                data-testid={'Find Another Activity'}
              >
                Find Another Activity
              </Button>
              <Button
                color="primary"
                className={classes.viewAccountButton}
                variant="contained"
                type="submit"
                onClick={() => navigateTo(accountUrl)}
                data-testid={'View Account'}
              >
                View Account
              </Button>
            </>
          )}
        </Box>

        {isParent && (
          <Link
            href={RegisterForNewChildUrl}
            className={classes.link}
            variant="body1"
            data-testid={'Register for New Child'}
          >
            Register for New Child
          </Link>
        )}
      </Box>
      <Box className={classes.shareBoxesWrapper}>
        <Typography variant="body1" style={{ padding: '0px 10px' }}>
          Share <b>{ActivityName}</b> with your friends and family via link, Twitter, or Facebook!
        </Typography>
        <Box className={classes.ShareButtonsWrapper}>
          <CopyButton />
          <ShareButtons />
        </Box>
      </Box>
    </Box>
  );
}

export default ThankYouSection;
