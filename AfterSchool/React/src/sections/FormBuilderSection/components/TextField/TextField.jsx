import React from 'react';
import { Grid, Typography, TextField as MuiTextField } from '@material-ui/core';

import useStyles from './styles';

export default function TextField({ formikProps, label }) {
  const classes = useStyles();
  const [typeClass, typeText] = [classes.fieldLabelRequired, '*'];

  return (
    <Grid className={classes.textFieldBox} xs={12} data-testid={`${label}-text-field`}>
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
        data-testid={`${label}-text-field-input`}
      >
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
            className: classes.helperText
          }}
        />
      </Grid>
    </Grid>
  );
}
