import { usePostRegistration } from 'redux/hooks';
import { useParams } from 'react-router-dom';
import { useSnackbar } from 'notistack';
import { Box } from '@material-ui/core';
import React, { useState } from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';

import useStyles from './styles';
import { withoutSignsPhoneNumber } from 'utils';
import { phoneValidationRegex } from 'constants/regex';
import TextField from 'components/Overrides/TextField';
import LoadingButton from 'components/Overrides/LoadingButton';

function PinSetupForm(props) {
  const classes = useStyles();
  const { setActiveStep, RegistrantType } = props;
  const { registrationUid } = useParams();
  const { enqueueSnackbar } = useSnackbar();
  const [saveDisabled, setSaveDisabled] = useState(false);
  const {
    updateData,
    data: { setupDto }
  } = usePostRegistration();

  const { IsPhoneSetup } = setupDto;

  const ValidationSchema = Yup.object().shape({
    phone: Yup.string().when([], {
      is: () => !IsPhoneSetup,
      then: () =>
        Yup.string()
          .matches(phoneValidationRegex, 'Phone number is not valid')
          .required('Phone Number is required'),
      otherwise: () => Yup.string().notRequired()
    }),
    pin: Yup.string()
      .min(4, 'Pin must be of 4 digits')
      .required('Pin is required'),
    confirmPin: Yup.string()
      .required('Please confirm your pin.')
      .oneOf([Yup.ref('pin')], 'Your pins do not match.')
  });

  const formik = useFormik({
    initialValues: {
      phone: '',
      pin: '',
      confirmPin: ''
    },
    validationSchema: ValidationSchema,
    onSubmit: async values => {
      setSaveDisabled(true);
      await updateData({
        registrationUid,
        phone: withoutSignsPhoneNumber(values.phone),
        pinCode: values.pin,
        peopleList: []
      }).then(apiErrors => {
        setSaveDisabled(false);
        if (apiErrors.length) return;
        if (RegistrantType === 'Parent') {
          setActiveStep(1);
        } else setActiveStep(2);
        enqueueSnackbar('Submitted.', { variant: 'success' });
      });
    }
  });

  const handleInputChange = (event, field) => {
    const inputValue = event.target.value;
    const formattedValue = inputValue.slice(0, 4);
    formik.setFieldValue(field, formattedValue);
  };

  const { errors, touched, handleSubmit, getFieldProps, values } = formik;

  return (
    <Box className={classes.pinSetupWrapper}>
      {!IsPhoneSetup && (
        <TextField
          id="phone"
          data-testid="phone-field"
          customLabel="Cell Phone Number"
          variant="outlined"
          autofill="false"
          fullWidth
          {...getFieldProps('phone')}
          error={!!(touched['phone'] && errors['phone'])}
          helperText={touched['phone'] && errors['phone']}
          required={true}
        />
      )}
      <TextField
        id="pinCode"
        customLabel="PIN Code"
        variant="outlined"
        data-testid="pin-field"
        fullWidth
        onChange={e => handleInputChange(e, 'pin')}
        value={values.pin}
        error={!!(touched['pin'] && errors['pin'])}
        helperText={touched['pin'] && errors['pin']}
        required={true}
        type="number"
      />
      <TextField
        id="confirmPinCode"
        customLabel="Confirm PIN Code"
        data-testid="confirm-pin-field"
        variant="outlined"
        fullWidth
        onChange={e => handleInputChange(e, 'confirmPin')}
        value={values.confirmPin}
        error={!!(touched['confirmPin'] && errors['confirmPin'])}
        helperText={touched['confirmPin'] && errors['confirmPin']}
        required={true}
        type="number"
      />
      <Box className={classes.pinSetupButtonsWrapper}>
        <LoadingButton
          color="primary"
          className={classes.saveButton}
          variant="contained"
          type="submit"
          data-testid="save-continue"
          onClick={() => handleSubmit()}
          text="Save & Continue"
          loading={saveDisabled}
        />
      </Box>
    </Box>
  );
}

export default PinSetupForm;
