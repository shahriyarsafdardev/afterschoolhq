import React from 'react';
import { Box, Typography, TextField } from '@material-ui/core';

import useStyles from './styles';

export default function TextAreaField(props) {
  const classes = useStyles();
  const { required = false, customLabel, helperText, ...fieldProps } = props;
  const [typeClass, typeText] = required
    ? [classes.fieldLabelRequired, '*']
    : [classes.fieldLabelOptional, '(Optional)'];

  return (
    <Box className={classes.outerWrapper}>
      <Typography variant="h6" className={classes.fieldLabel}>
        {customLabel}
        &nbsp;
        <Typography component="span" className={typeClass}>
          {typeText}
        </Typography>
      </Typography>

      <TextField
        className={classes.formControlClass}
        fullWidth
        variant="outlined"
        {...fieldProps}
        helperText={helperText || `${props.value?.length || 0}/4000`}
      />
    </Box>
  );
}
