import React from 'react';
import { Box } from '@material-ui/core';
import { withStyles } from '@material-ui/core/styles';
import LinearProgress from '@material-ui/core/LinearProgress';
import useStyles from './styles';

export default function ProgressBar({ activeGroup, totalGroups }) {
  const classes = useStyles();
  const BorderLinearProgress = withStyles((theme) => ({
    root: {
      height: 20,
      borderRadius: 10,
      width: '100%',
      maxWidth: '366px',
    },
    colorPrimary: {
      backgroundColor: theme.palette.info,
    },
  }))(LinearProgress);

  const currentProgress = ((activeGroup + 1) / totalGroups) * 100;
  const progressPosition = (currentProgress < 50 ? currentProgress - 6 : currentProgress - 10) / 2;
  return (
    <Box className={classes.progressWrapper}>
      <BorderLinearProgress variant="determinate" value={currentProgress} />
      <Box className={classes.progressNumber} style={{ left: `${progressPosition}%` }}>
        {`${activeGroup + 1}/${totalGroups}`}
      </Box>
    </Box>
  );
}
