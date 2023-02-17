import React from 'react';
import { Grid, Typography, TextField as MuiTextField } from '@material-ui/core';

import useStyles from './styles';

export default function TextField({
  formik, question,
}) {
  const classes = useStyles();
  const { ID, Text: label, IsRequired: required = false } = question;
  const [typeClass, typeText] = required ? [classes.fieldLabelRequired, '*'] : [classes.fieldLabelOptional, '(Optional)'];
  return (
    <Grid className={classes.textFieldBox} xs={12}>
      <Grid xs={12} md={4} item className={classes.titleGrid}>
        <Typography variant="h6" className={classes.fieldLabel}>
          {label}
&nbsp;
          <Typography className={typeClass}>{typeText}</Typography>
        </Typography>
      </Grid>
      <Grid xs={12} md={8} item className={classes.textFieldGrid}>
        <MuiTextField
          className={classes.formControlClass}
          fullWidth
          variant="outlined"
          {...formik.getFieldProps(question.ID)}
          error={formik.touched[ID] && formik.errors[ID]}
          helperText={formik.touched[ID] && formik.errors[ID]}
        />
      </Grid>
    </Grid>
  );
}
