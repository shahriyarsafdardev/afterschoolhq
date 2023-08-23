import React from 'react';
import { Box, Typography, TextField as MuiTextField, FormHelperText } from '@material-ui/core';

import useStyles from './styles';

export default function TextAreaField(props) {
  const classes = useStyles();
  const { required = false, customLabel, error, helperText, ...fieldProps } = props;
  const [typeClass, typeText] = required
    ? [classes.fieldLabelRequired, '*']
    : [classes.fieldLabelOptional, '(Optional)'];

  return (
    <Box className={classes.outerWrapper}>
      <Box className={classes.fieldLabelWrapper}>
        <Typography variant="h6" className={classes.fieldLabel}>
          {customLabel}
          &nbsp;
          <Typography component="span" className={typeClass}>
            {typeText}
          </Typography>
        </Typography>
      </Box>

      <Box className={classes.textfieldWrapper}>
        <MuiTextField
          className={classes.formControlClass}
          fullWidth
          variant="outlined"
          {...fieldProps}
        />
        <Box className={classes.flexCenter}>
          <FormHelperText
            data-testid={`${customLabel}s-error`}
            className={classes.helperText}
            error={error}
          >
            {helperText || `${props.value?.length || 0}/4000`}
          </FormHelperText>
        </Box>
      </Box>
    </Box>
  );
}
