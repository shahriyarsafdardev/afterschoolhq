import * as Yup from 'yup';
import { Box, Button, CircularProgress } from '@material-ui/core';
import React, { useEffect, useState } from 'react';
import useStyles from './styles';
import TextField from 'components/Overrides/TextField';
import { useFormik } from 'formik';
import { phoneValidationRegex } from 'constants/regex';
import { usePostRegistration } from 'redux/hooks';
import { useParams } from 'react-router-dom';
import { useSnackbar } from 'notistack';

function PinSetupForm(props) {
  const classes = useStyles();
  const { setActiveStep } = props;
  const { registrationUid } = useParams();
  const { enqueueSnackbar } = useSnackbar();
  const [saveDisabled, setSaveDisabled] = useState(false);
  const {
    msgs,
    updateData,
    data: { setupDto }
  } = usePostRegistration();

  const { IsPhoneSetup } = setupDto;

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

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
      .oneOf([Yup.ref('pin')], 'Your pin do not match.')
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
        phone: values.phone,
        pinCode: values.pin,
        peopleList: []
      }).then(apiErrors => {
        setSaveDisabled(false);
        if (apiErrors.length) return;

        setActiveStep(1);
        enqueueSnackbar('Submitted.', { variant: 'success' });
      });
    }
  });

  const handleInputChange = (event, field) => {
    const inputValue = event.target.value;
    const formattedValue = inputValue.slice(0, 4); // Limit to 4 digits
    formik.setFieldValue(field, formattedValue);
  };

  const { errors, touched, handleSubmit, getFieldProps, values } = formik;

  return (
    <Box
      style={{
        display: 'flex',
        flexDirection: 'column',
        gap: '20px'
      }}
    >
      {!IsPhoneSetup && (
        <TextField
          id="phone"
          customLabel="Cell Phone Number"
          variant="outlined"
          autofill="false"
          style={{ width: '100%' }}
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
        style={{ width: '100%' }}
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
        variant="outlined"
        style={{ width: '100%' }}
        onChange={e => handleInputChange(e, 'confirmPin')}
        value={values.confirmPin}
        error={!!(touched['confirmPin'] && errors['confirmPin'])}
        helperText={touched['confirmPin'] && errors['confirmPin']}
        required={true}
        type="number"
      />
      <Box style={{ maxWidth: '600px', margin: 'auto', textAlign: 'center', paddingTop: '40px' }}>
        <Button
          color="primary"
          className={classes.saveButton}
          variant="contained"
          type="submit"
          data-testid={'Save & continue'}
          onClick={() => handleSubmit()}
          disabled={saveDisabled}
        >
          {saveDisabled ? (
            <>
              {'Loading . . .'} <CircularProgress size={'20px'} />
            </>
          ) : (
            'Save & Continue'
          )}
        </Button>
      </Box>
    </Box>
  );
}

export default PinSetupForm;
