import * as Yup from 'yup';
import { useFormik } from 'formik';
import React, { useState } from 'react';
import { useSnackbar } from 'notistack';
import { Box, Button } from '@material-ui/core';

import useStyles from './styles';
import { withoutSignsPhoneNumber } from 'utils';
import { useManageAPAccess } from 'redux/hooks';
import TextField from 'components/Overrides/TextField';
import LoadingButton from 'components/Overrides/LoadingButton';
import { phoneValidationRegex, numericValidationRegex } from 'constants/regex';

function PinSetupForm(props) {
  const classes = useStyles();
  const { isPhoneSetup } = props;
  const { enqueueSnackbar } = useSnackbar();
  const [saveDisabled, setSaveDisabled] = useState(false);

  const { createPinForParent, data } = useManageAPAccess();

  const ValidationSchema = Yup.object().shape({
    phone: Yup.string().when([], {
      is: () => !isPhoneSetup,
      then: () =>
        Yup.string()
          .matches(phoneValidationRegex, 'Phone number is not valid')
          .required('Phone Number is required'),
      otherwise: () => Yup.string().notRequired()
    }),
    pin: Yup.string()
      .required('Pin is required')
      .matches(numericValidationRegex, 'Pin should be a numeric value')
      .min(4, 'Pin must be of 4 digits'),
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
    onSubmit: async (values, { resetForm }) => {
      setSaveDisabled(true);
      await createPinForParent(withoutSignsPhoneNumber(values.phone), values.pin, data).then(
        msgs => {
          if (!msgs.length) {
            enqueueSnackbar('Submitted.', { variant: 'success' });
            resetForm();
          }
          setSaveDisabled(false);
        }
      );
    }
  });

  const handleInputChange = (event, field) => {
    const inputValue = event.target.value;
    const formattedValue = inputValue.slice(0, 4);
    formik.setFieldValue(field, formattedValue);
  };

  const { errors, touched, handleSubmit, getFieldProps, values, resetForm } = formik;

  const submitPinCodeSetup = e => {
    e.preventDefault();
    handleSubmit();
  };

  return (
    <Box className={classes.pinSetupWrapper}>
      {!isPhoneSetup && (
        <TextField
          id="phone"
          size="small"
          required={true}
          autofill="false"
          variant="outlined"
          data-testid="phone-field"
          {...getFieldProps('phone')}
          customLabel="Cell Phone Number"
          className={classes.textField}
          error={!!(touched['phone'] && errors['phone'])}
          helperText={touched['phone'] && errors['phone']}
        />
      )}
      <TextField
        size="small"
        id="pinCode"
        type="password"
        required={true}
        variant="outlined"
        value={values.pin}
        customLabel="PIN Code"
        data-testid="pin-field"
        className={classes.textField}
        onChange={e => handleInputChange(e, 'pin')}
        error={!!(touched['pin'] && errors['pin'])}
        helperText={touched['pin'] && errors['pin']}
      />
      <TextField
        size="small"
        type="password"
        required={true}
        variant="outlined"
        id="confirmPinCode"
        value={values.confirmPin}
        className={classes.textField}
        customLabel="Confirm PIN Code"
        data-testid="confirm-pin-field"
        onChange={e => handleInputChange(e, 'confirmPin')}
        error={!!(touched['confirmPin'] && errors['confirmPin'])}
        helperText={touched['confirmPin'] && errors['confirmPin']}
      />
      <Box className={classes.pinSetupButtonsWrapper}>
        <Button
          color="primary"
          variant="outlined"
          className={classes.cancelButton}
          data-testid="cancel-button"
          onClick={() => {
            resetForm();
          }}
        >
          Cancel
        </Button>
        <LoadingButton
          type="submit"
          color="primary"
          variant="contained"
          text="Save"
          loading={saveDisabled}
          data-testid="save"
          className={classes.saveButton}
          onClick={submitPinCodeSetup}
        />
      </Box>
    </Box>
  );
}

export default PinSetupForm;
