import React from 'react';
import { Box, Typography, TextareaAutosize, FormHelperText } from '@material-ui/core';

import useStyles from './styles';

export default function TextAreaQuestion({ formik, question }) {
  const classes = useStyles();
  const { ID, Text: label, IsRequired: required = false, QuestionDataTypeId } = question;
  const { errors, touched } = formik;
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
        <TextareaAutosize
          data-testid={`${label}-${QuestionDataTypeId}`}
          minRows={4}
          className={classes.formControlClass}
          variant="outlined"
          {...formik.getFieldProps(question.ID)}
          error={formik.touched[ID] && formik.errors[ID]}
          helpertext={formik.touched[ID] && formik.errors[ID]}
        />
        {touched[ID] && errors[ID] && (
          <Box className={classes.flexCenter}>
            <FormHelperText
              data-testid={`${label}-${QuestionDataTypeId}-error`}
              className={classes.maxWidth}
              error
            >
              {errors[ID]}
            </FormHelperText>
          </Box>
        )}
      </Box>
    </Box>
  );
}
