import React from 'react';
import { Box, Typography } from '@material-ui/core';
import CopyUrl from 'assets/icon-ios7-world-outline.svg';
import Icon from '@material-ui/core/Icon';
import useStyles from './styles';

function ShareButton(props) {
  const classes = useStyles();
  const { acticityUrl, activityName, appName, site } = props;

  const facecbookShareUrl = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(
    acticityUrl
  )}&quote=${encodeURIComponent('textToShare')}`;

  const twitterShareUrl = `http://twitter.com/share?text=${encodeURIComponent(
    `Check out ${activityName} on ${appName}`
  )}&url=${encodeURIComponent(acticityUrl)}&hashtags=Education,AfterschoolActivities,AfterSchoolHQ`;

  const handleClick = () => {
    let shareUrl = '';
    if (site === 'Facebook') shareUrl = facecbookShareUrl;
    else if (site === 'Twitter') shareUrl = twitterShareUrl;

    // window.open(shareUrl, '_blank', 'noopener noreferrer');
    window.open(shareUrl, 'FacebookShareDialog', 'width=626,height=436,top=200,left=200');
  };

  return (
    <Box className={classes.shareBox} onClick={handleClick}>
      <Icon className={classes.icon}>
        <img className={classes.icon} src={CopyUrl} />
      </Icon>
      <Typography>{site}</Typography>
    </Box>
  );
}

export default ShareButton;
