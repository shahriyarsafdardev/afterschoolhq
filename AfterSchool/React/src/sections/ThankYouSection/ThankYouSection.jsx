import { useSnackbar } from 'notistack';
import React, { useState, useEffect } from 'react';
import { Box, Button, Link, Typography, Icon } from '@material-ui/core';

import utils from 'utils';
import useStyles from './styles';
import apiRoutes from 'routes';
import ShareButton from 'components/ShareButton';
import { useUser, useRegistration } from 'redux/hooks';
import CopyUrlIcon from 'assets/icon-ios7-world-outline.svg';

function ThankYouSection() {
  const classes = useStyles();
  const { navigateTo } = utils;
  const { accountUrl } = apiRoutes;
  const { enqueueSnackbar } = useSnackbar();
  const [copySuccess, setCopySuccess] = useState('Copy URL');

  const {
    user: { firstName }
  } = useUser();

  const {
    activity: {
      activityUrl,
      RegistrantType,
      Name: activityName,
      findAnotherActivityUrl,
      RegisterForNewChildUrl
    }
  } = useRegistration();

  useEffect(() => {
    //Restrict user not to go back
    window.history.pushState(null, '', window.location.href);
    window.onpopstate = function() {
      window.history.pushState(null, '', window.location.href);
    };
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
    <Box onClick={() => copyToClipBoard(activityUrl)} className={classes.shareBox} title="Copy URL">
      <Icon className={classes.icon}>
        <img className={classes.icon} src={CopyUrlIcon} />
      </Icon>
      <Typography>{copySuccess}</Typography>
    </Box>
  );

  const ShareButtons = () =>
    ['Facebook', 'Twitter'].map(site => (
      <ShareButton key={site} site={site} activityName={activityName} activityUrl={activityUrl} />
    ));

  return (
    <Box className={classes.wrapper}>
      <Box className={classes.thankYouWrapper}>
        <Typography variant="h4">Thank You!</Typography>
        <Typography variant="body1">
          <b>{firstName}</b> has been registered for <b>{activityName}</b>
        </Typography>
        <Box>
          <Button
            color="primary"
            variant="outlined"
            className={classes.anotherActivityButton}
            onClick={() => navigateTo(findAnotherActivityUrl)}
          >
            Find Another Activity
          </Button>
          <Button
            color="primary"
            className={classes.viewAccountButton}
            variant="contained"
            type="submit"
            onClick={() => navigateTo(accountUrl)}
          >
            View Account
          </Button>
        </Box>

        {RegistrantType === 'Parent' && (
          <Link href={RegisterForNewChildUrl} className={classes.link} variant="body1">
            Register for New Child
          </Link>
        )}
      </Box>
      <Box className={classes.shareBoxesWrapper}>
        <Typography variant="body1" style={{ padding: '0px 10px' }}>
          Share <b>{activityName}</b> with your friends and family via link, Twitter, or Facebook!
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
