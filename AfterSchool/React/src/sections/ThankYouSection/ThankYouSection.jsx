import useStyles from './styles';
import React, { useState } from 'react';
import Icon from '@material-ui/core/Icon';
import ShareButton from 'components/ShareButton';
import CopyUrl from 'assets/icon-ios7-world-outline.svg';
import { Box, Button, Link, Typography } from '@material-ui/core';

function ThankYouSection(props) {
  const classes = useStyles();
  const [copySuccess, setCopySuccess] = useState('Copy URL');
  const {
    userName = 'Margo',
    activityName = 'Intro to Martial Arts',
    activityUrl = 'http://localhost:54849/AhsansOrganization/activity/introtomartialarts',
    appName = 'AfterSchool HQ'
  } = props;

  const copyToClipBoard = async copyMe => {
    try {
      await navigator.clipboard.writeText(copyMe);
      setCopySuccess('Copied!');
    } catch (err) {
      setCopySuccess('Failed to copy!');
    }
  };

  return (
    <Box className={classes.wrapper}>
      <Box className={classes.thankYouWrapper}>
        <Typography variant="h4">Thank You!</Typography>
        <Typography variant="body1">
          <b>{userName}</b> has been registered for <b>{activityName}</b>
        </Typography>
        <Box>
          <Button color="primary" variant="outlined" className={classes.anotherActivityButton}>
            Find Another Activity
          </Button>
          <Button
            color="primary"
            className={classes.viewAccountButton}
            variant="contained"
            type="submit"
          >
            View Account
          </Button>
        </Box>
        <Link href="#" className={classes.link} variant="body1">
          Register for New Child
        </Link>
      </Box>
      <Box className={classes.shareBoxesWrapper}>
        <Typography variant="body1">
          Share <b>{activityName}</b> with your friends and family via link, Twitter, or Facebook!
        </Typography>
        <Box className={classes.ShareButtonsWrapper}>
          <Box
            onClick={() => copyToClipBoard(activityUrl)}
            className={classes.shareBox}
            title="Copy URL"
          >
            <Icon className={classes.icon}>
              <img className={classes.icon} src={CopyUrl} />
            </Icon>
            <Typography>{copySuccess}</Typography>
          </Box>
          <ShareButton
            site="Facebook"
            activityName={activityName}
            activityUrl={activityUrl}
            appName={appName}
          />
          <ShareButton
            site="Twitter"
            activityName={activityName}
            activityUrl={activityUrl}
            appName={appName}
          />
        </Box>
      </Box>
    </Box>
  );
}

export default ThankYouSection;
