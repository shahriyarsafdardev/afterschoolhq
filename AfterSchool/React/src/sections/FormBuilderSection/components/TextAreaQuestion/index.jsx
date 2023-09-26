import React from 'react';
import { Box, Typography, TextareaAutosize, FormHelperText } from '@material-ui/core';

import useStyles from './styles';

export default function TextAreaQuestion({ formikProps, label }) {
  const classes = useStyles();
  const { error, value } = formikProps;

  return (
    <Box className={classes.outerWrapper}>
      <Box className={classes.fieldLabelWrapper}>
        <Typography variant="h6" className={classes.fieldLabel}>
          {label}
        </Typography>
      </Box>

      <Box className={classes.textfieldWrapper}>
        <TextareaAutosize
          data-testid={`${label}`}
          minRows={4}
          className={classes.formControlClass}
          variant="outlined"
          {...formikProps}
        />
        <Box className={classes.flexCenter}>
          <FormHelperText
            data-testid={`${label}s-error`}
            className={classes.helperText}
            error={error}
          >
            {`${value?.length || 0}/400`}
          </FormHelperText>
        </Box>
      </Box>
    </Box>
  );
}
