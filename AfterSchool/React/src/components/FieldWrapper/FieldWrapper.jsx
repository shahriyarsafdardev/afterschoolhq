import React from 'react';
import { Typography, Box } from '@material-ui/core';

import useStyles from './styles';

export default function FieldWrapper({ label, isRequired = false, children }) {
  const classes = useStyles();
  const [typeClass, typeText] = isRequired
    ? [classes.fieldLabelRequired, '*']
    : [classes.fieldLabelOptional, '(Optional)'];
  return (
    <Box className={classes.outerWrapper}>
      <Box className={classes.textFieldWrapper}>
        <Typography variant="h6" className={classes.fieldLabel}>
          {label}
          &nbsp;
        </Typography>
        <Typography component="span" className={typeClass}>
          {typeText}
        </Typography>
      </Box>
      <Box className={classes.childWrapper}>{children}</Box>
    </Box>
  );
}
