import React from 'react';
import { Box, Typography } from '@material-ui/core';

import useStyles from './styles';
import apiRoutes from 'routes';
import Icon from '@material-ui/core/Icon';
import facebookIcon from 'assets/social-facebook.svg';
import twitterIcon from 'assets/social-twitter-outline-1.svg';

function ShareButton(props) {
  const classes = useStyles();
  const { facebookShareUrl, twitterShareUrl } = apiRoutes;
  const { activityUrl, activityName, appName = 'AfterSchool HQ', platformName } = props;

  const platformData = {
    Facebook: {
      shareUrl: facebookShareUrl(activityUrl),
      icon: facebookIcon
    },
    Twitter: {
      shareUrl: twitterShareUrl(activityName, appName, activityUrl),
      icon: twitterIcon
    }
  };

  const { shareUrl, icon } = platformData[platformName];
  const handleClick = () => window.open(shareUrl, '_blank', 'noopener noreferrer');

  return (
    <Box className={classes.shareBox} onClick={handleClick} data-testid={platformName}>
      <Icon className={classes.icon}>
        <img className={classes.icon} src={icon} />
      </Icon>
      <Typography>{platformName}</Typography>
    </Box>
  );
}

export default ShareButton;
