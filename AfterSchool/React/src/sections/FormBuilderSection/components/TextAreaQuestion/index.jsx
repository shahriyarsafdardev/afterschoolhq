import React from 'react';
import { Box, Grid, Typography, TextareaAutosize, FormHelperText } from '@material-ui/core';

import useStyles from './styles';

export default function TextAreaQuestion({ formikProps, label }) {
  const classes = useStyles();
  const { error, errorMsg } = formikProps;
  return (
    <Grid className={classes.textFieldBox} xs={12}>
      <Grid xs={12} md={4} item className={classes.titleGrid}>
        <Typography variant="h6" className={classes.fieldLabel}>
          {label}
          &nbsp;
        </Typography>
      </Grid>
      <Grid xs={12} md={8} item className={classes.textFieldGrid}>
        <>
          <TextareaAutosize
            data-testid={`${label}`}
            minRows={4}
            className={classes.formControlClass}
            variant="outlined"
            {...formikProps}
          />
          {error && (
            <Box className={classes.flexCenter}>
              <FormHelperText className={classes.maxWidth} error>
                {errorMsg}
              </FormHelperText>
            </Box>
          )}
        </>
      </Grid>
    </Grid>
  );
}
