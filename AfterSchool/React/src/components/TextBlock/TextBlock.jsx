import React from 'react';
import { Grid, Typography } from '@material-ui/core';

import useStyles from './styles';

export default function TextBlock({ question }) {
  const classes = useStyles();
  const { Description } = question;
  return (
    <Grid className={classes.textFieldBox} xs={12}>
      <Grid xs={12} md={4} item className={classes.titleGrid}></Grid>
      <Grid xs={12} md={8} item className={classes.textFieldGrid}>
        <Typography className={classes.fieldLabel}>{Description}</Typography>
      </Grid>
    </Grid>
  );
}
