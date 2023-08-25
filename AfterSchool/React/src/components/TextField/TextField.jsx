import React from 'react';
import { Typography, TextField as MuiTextField, Box } from '@material-ui/core';

import useStyles from './styles';

export default function TextField({ formik, question, type = 'text' }) {
  const classes = useStyles();
  const { ID, Text: label, IsRequired: required = false, QuestionDataTypeId } = question;
  const [typeClass, typeText] = required
    ? [classes.fieldLabelRequired, '*']
    : [classes.fieldLabelOptional, '(Optional)'];

  return (
    <Box className={classes.outerWrapper}>
      <Box className={classes.fieldLabelWrapper}>
        <Typography variant="h6" className={classes.fieldLabel}>
          {label}
          &nbsp;
        </Typography>
        <Typography component="span" className={typeClass}>
          {typeText}
        </Typography>
      </Box>

      <Box className={classes.textfieldWrapper}>
        <MuiTextField
          data-testid={`${label}-${QuestionDataTypeId}-form-control`}
          inputProps={{
            'data-testid': `${label}-${QuestionDataTypeId}`
          }}
          className={classes.formControlClass}
          fullWidth
          type={type}
          variant="outlined"
          {...formik.getFieldProps(question.ID)}
          error={!!(formik.touched[ID] && formik.errors[ID])}
          helperText={formik.touched[ID] && formik.errors[ID]}
          FormHelperTextProps={{
            'data-testid': `${label}-${QuestionDataTypeId}-error`
          }}
        />
      </Box>
    </Box>
  );
}
