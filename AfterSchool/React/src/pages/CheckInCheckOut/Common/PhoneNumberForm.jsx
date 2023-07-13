import React from 'react';
import { Box, Button, Typography } from '@material-ui/core';
import useStyles from '../Common/styles';
import TextField from 'components/Overrides/TextField';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import { phoneValidationRegex } from 'constants/regex';

function PhoneNumberForm({ setSearchParams }) {
  const classes = useStyles();

  const handleSubmit = () => {
    setSearchParams(`phone=${values.phone}`);
  };

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
    onSubmit: () => {
      handleSubmit();
    }
  });

  const { values, getFieldProps, touched, errors, isValid } = formik;

  return (
    <Box className={classes.phoneForm}>
      <Typography className={classes.headerText}>Please enter your phone number.</Typography>
      <TextField
        value={values.phone}
        type="number"
        {...getFieldProps('phone')}
        error={!!(touched['phone'] && errors['phone'])}
        helperText={touched['phone'] && errors['phone']}
        fullWidth
      />
      <Button
        onClick={handleSubmit}
        className={classes.buttonNext}
        variant="contained"
        color="primary"
        disabled={!isValid}
      >
        Next
      </Button>
    </Box>
  );
}

export default PhoneNumberForm;
