import { CircularProgress, Box } from '@material-ui/core';
import React from 'react';
import useStyles from './styles';

export default function Loader() {
  const classes = useStyles();
  return (
    <Box className={classes.loader}>
      <CircularProgress />
    </Box>
  );
}
