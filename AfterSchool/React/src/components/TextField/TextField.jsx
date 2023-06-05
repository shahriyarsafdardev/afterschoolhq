import React from 'react';
import { Grid, Typography, TextField as MuiTextField } from '@material-ui/core';

import useStyles from './styles';

export default function TextField({ formik, question, type = 'text' }) {
  const classes = useStyles();
  const { ID, Text: label, IsRequired: required = false, QuestionDataTypeId } = question;
  const [typeClass, typeText] = required
    ? [classes.fieldLabelRequired, '*']
    : [classes.fieldLabelOptional, '(Optional)'];
  return (
    <Grid
      className={classes.textFieldBox}
      xs={12}
      data-testid={`${label}-${QuestionDataTypeId}-text-field`}
    >
      <Grid xs={12} md={4} item className={classes.titleGrid}>
        <Typography variant="h6" className={classes.fieldLabel}>
          {label}
          &nbsp;
          <Typography className={typeClass}>{typeText}</Typography>
        </Typography>
      </Grid>
      <Grid
        xs={12}
        md={8}
        item
        className={classes.textFieldGrid}
        data-testid={`${label}-${QuestionDataTypeId}-text-field-input`}
      >
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
          error={formik.touched[ID] && formik.errors[ID]}
          helperText={formik.touched[ID] && formik.errors[ID]}
        />
      </Grid>
    </Grid>
  );
}
