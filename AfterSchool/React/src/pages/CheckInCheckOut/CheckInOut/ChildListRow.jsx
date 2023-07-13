import { Box, Typography } from '@material-ui/core';
import React from 'react';
import useStyles from '../Common/styles';

export default function ChildListRow({ student }) {
  const classes = useStyles();
  return (
    <Box>
      <Typography className={classes.fontBold}>{student?.StudentName}</Typography>
      <Typography className={classes.fontLight}>{student?.ActivityName}</Typography>
    </Box>
  );
}
