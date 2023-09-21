import React from 'react';
import { Box, Typography, TextField as MuiTextField } from '@material-ui/core';

import useStyles from './styles';

export default function TextField({ formikProps, label }) {
  const classes = useStyles();
  const [typeClass, typeText] = [classes.fieldLabelRequired, '*'];

  return (
    <Box className={classes.outerWrapper} data-testid={`${label}-text-field`}>
      <Box className={classes.fieldLabelWrapper}>
        <Typography variant="h6" className={classes.fieldLabel}>
          {label}
          &nbsp;
        </Typography>
        <Typography component="span" className={typeClass}>
          {typeText}
        </Typography>
      </Box>

      <Box className={classes.textfieldWrapper} data-testid={`${label}-text-field-input`}>
        <MuiTextField
          data-testid={`${label}-form-control`}
          inputProps={{
            'data-testid': `${label}`,
            maxLength: 400
          }}
          className={classes.formControlClass}
          fullWidth
          variant="outlined"
          {...formikProps}
          FormHelperTextProps={{
            className: classes.helperText,
            'data-testid': `${label}-error`
          }}
        />
      </Box>
    </Box>
  );
}
