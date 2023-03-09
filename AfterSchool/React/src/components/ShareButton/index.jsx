import React from 'react';
import { Box, Typography } from '@material-ui/core';

import useStyles from './styles';
import apiRoutes from 'routes/apiRoutes';
import Icon from '@material-ui/core/Icon';
import facebookIcon from 'assets/social-facebook.svg';
import twitterIcon from 'assets/social-twitter-outline-1.svg';

function ShareButton(props) {
  const classes = useStyles();
  const { facebookShareUrl, twitterShareUrl } = apiRoutes;
  const { activityUrl, activityName, appName = 'AfterSchool HQ', site } = props;

  const siteData = {
    Facebook: {
      shareUrl: facebookShareUrl(activityUrl),
      icon: facebookIcon
    },
    Twitter: {
      shareUrl: twitterShareUrl(activityName, appName, activityUrl),
      icon: twitterIcon
    }
  };

  const { shareUrl, icon } = siteData[site];
  const handleClick = () => window.open(shareUrl, '_blank', 'noopener noreferrer');

  return (
    <Box className={classes.shareBox} onClick={handleClick}>
      <Icon className={classes.icon}>
        <img className={classes.icon} src={icon} />
      </Icon>
      <Typography>{site}</Typography>
    </Box>
  );
}

export default ShareButton;
