import React from 'react';
import { Typography, Box } from '@material-ui/core';

import useStyles from './styles';

export default function FieldWrapper({ label, isRequired = false, children }) {
  const classes = useStyles();
  const [typeClass, typeText] = isRequired
    ? [classes.fieldLabelRequired, '*']
    : [classes.fieldLabelOptional, '(Optional)'];
  return (
    <Box style={{ maxWidth: '675px', margin: 'auto' }}>
      <Box style={{ display: 'flex' }}>
        <Typography variant="h6" className={classes.fieldLabel}>
          {label}
          &nbsp;
          <Typography component="span" className={typeClass}>
            {typeText}
          </Typography>
        </Typography>
      </Box>
      <Box style={{ margin: 'auto' }}>{children}</Box>
    </Box>
  );
}
