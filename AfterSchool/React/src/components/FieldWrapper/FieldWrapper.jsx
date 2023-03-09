import React from 'react';
import { Grid, Typography } from '@material-ui/core';

import useStyles from './styles';

export default function FieldWrapper({ label, isRequired = false, children }) {
  const classes = useStyles();
  const [typeClass, typeText] = isRequired
    ? [classes.fieldLabelRequired, '*']
    : [classes.fieldLabelOptional, '(Optional)'];
  return (
    <Grid className={classes.textFieldBox} xs={12}>
      <Grid xs={12} md={4} item className={classes.titleGrid}>
        <Typography variant="h6" className={classes.fieldLabel}>
          {label}
          &nbsp;
          <Typography className={typeClass}>{typeText}</Typography>
        </Typography>
      </Grid>
      <Grid xs={12} md={8} item className={classes.textFieldGrid}>
        {children}
      </Grid>
    </Grid>
  );
}
