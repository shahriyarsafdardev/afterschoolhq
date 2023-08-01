import React from 'react';
import { Box, Typography } from '@material-ui/core';
import useStyles from '../Common/styles';
import TextField from 'components/Overrides/TextField';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import LoadingButton from 'components/Overrides/LoadingButton';

import { phoneValidationRegex } from 'constants/regex';

function PhoneNumberForm({
  handleSubmit: submitHandler,
  headerText = 'Please enter your phone number.',
  subHeaderText = '',
  submitting = false
}) {
  const classes = useStyles();

  const ValidationSchema = Yup.object().shape({
    phone: Yup.string()
      .matches(phoneValidationRegex, 'Phone number is not valid')
      .required('Phone Number is required')
  });

  const formik = useFormik({
    initialValues: {
      phone: ''
    },
    validationSchema: ValidationSchema,
    onSubmit: values => {
      submitHandler(values.phone);
    }
  });

  const { values, getFieldProps, touched, errors, isValid, handleSubmit } = formik;

  return (
    <Box className={classes.phoneForm}>
      <Typography className={classes.headerText}>{headerText}</Typography>
      {subHeaderText && <Typography className={classes.subHeaderText}>{subHeaderText}</Typography>}
      <TextField
        value={values.phone}
        type="text"
        {...getFieldProps('phone')}
        error={!!(touched['phone'] && errors['phone'])}
        helperText={touched['phone'] && errors['phone']}
        fullWidth
      />
      <LoadingButton
        type="submit"
        onClick={handleSubmit}
        variant="contained"
        color="primary"
        text="Next"
        disabled={!isValid}
        loading={submitting}
        className={classes.buttonNext}
      />
    </Box>
  );
}

export default PhoneNumberForm;
