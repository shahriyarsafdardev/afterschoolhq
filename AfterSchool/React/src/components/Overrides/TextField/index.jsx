import React from 'react';
import { Typography, TextField as MuiTextField, Box } from '@material-ui/core';

import useStyles from './styles';

export default function TextField(props) {
  const classes = useStyles();
  const { required = false, customLabel, ...fieldProps } = props;
  const [typeClass, typeText] = required
    ? [classes.fieldLabelRequired, '*']
    : [classes.fieldLabelOptional, '(Optional)'];

  return (
    <Box className={classes.outerWrapper}>
      {customLabel && (
        <Box className={classes.fieldLabelWrapper}>
          <Typography variant="h6" className={classes.fieldLabel}>
            {customLabel}
            &nbsp;
            <Typography component="span" className={typeClass}>
              {typeText}
            </Typography>
          </Typography>
        </Box>
      )}

      <Box className={classes.textfieldWrapper}>
        <MuiTextField
          className={classes.formControlClass}
          fullWidth
          variant="outlined"
          {...fieldProps}
        />
      </Box>
    </Box>
  );
}
