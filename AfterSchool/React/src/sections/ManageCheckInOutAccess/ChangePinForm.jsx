import * as Yup from 'yup';
import { useFormik } from 'formik';
import { useSnackbar } from 'notistack';
import React, { useState } from 'react';
import { Box, Button, Divider, Typography, Link } from '@material-ui/core';

import apiRoutes from 'routes';
import useStyles from './styles';
import { useManageAPAccess } from 'redux/hooks';
import TextField from 'components/Overrides/TextField';
import { numericValidationRegex } from 'constants/regex';
import LoadingButton from 'components/Overrides/LoadingButton';

function ChangePinForm(props) {
  const classes = useStyles();
  const { closeModal } = props;
  const { requestPinResetUrl } = apiRoutes;

  const { enqueueSnackbar } = useSnackbar();
  const [saveDisabled, setSaveDisabled] = useState(false);
  const { updateExistingPin } = useManageAPAccess();

  const ValidationSchema = Yup.object().shape({
    currentPin: Yup.string()
      .required('Pin is required')
      .matches(numericValidationRegex, 'Pin should be a numeric value')
      .min(4, 'Pin must be of 4 digits'),
    newPin: Yup.string()
      .required('Pin is required')
      .matches(numericValidationRegex, 'Pin should be a numeric value')
      .min(4, 'Pin must be of 4 digits'),
    confirmPin: Yup.string()
      .required('Please confirm your pin.')
      .oneOf([Yup.ref('newPin')], 'Your pins do not match.')
  });

  const formik = useFormik({
    initialValues: {
      newPin: '',
      confirmPin: '',
      currentPin: ''
    },
    validationSchema: ValidationSchema,
    onSubmit: async values => {
      setSaveDisabled(true);
      await updateExistingPin(values.currentPin, values.newPin).then(msgs => {
        if (!msgs.length) {
          resetForm();
          closeModal();
          enqueueSnackbar('Submitted.', { variant: 'success' });
        }
        setSaveDisabled(false);
      });
    }
  });

  const { errors, touched, handleSubmit, setFieldValue, getFieldProps, values, resetForm } = formik;

  const handleInputChange = (event, field) => {
    const inputValue = event.target.value;
    const formattedValue = inputValue.slice(0, 4);
    setFieldValue(field, formattedValue);
  };

  const handleChangePin = e => {
    e.preventDefault();
    handleSubmit();
  };

  const formField = ({ name, label, value }) => {
    const shouldShowForgotPin = name === 'currentPin';

    return (
      <Box>
        <TextField
          fullWidth
          name={name}
          value={value}
          type="password"
          {...getFieldProps(name)}
          data-testid={`${name}-change-pin-field`}
          onChange={e => handleInputChange(e, name)}
          error={!!(touched && touched[name] && errors[name])}
          helperText={touched && touched[name] && errors[name]}
        />
        <Box className={classes.fieldLabels}>
          <Typography>{label}</Typography>
          {shouldShowForgotPin && (
            <Link
              rel="noopener"
              target="_blank"
              href={requestPinResetUrl}
              data-testid={`forgot-pin-link`}
              className={classes.forgotPinBtn}
            >
              Forgot PIN Code?
            </Link>
          )}
        </Box>
      </Box>
    );
  };

  return (
    <Box>
      <form onSubmit={handleChangePin}>
        <Box className={classes.pinSetupWrapper}>
          {formField({ name: 'currentPin', label: 'Current PIN Code', value: values.currentPin })}
          {formField({ name: 'newPin', label: 'PIN Code', value: values.newPin })}
          {formField({ name: 'confirmPin', label: 'Confirm PIN Code', value: values.confirmPin })}
        </Box>
        <Divider className={classes.divider} />
        <Box className={classes.modalButtons}>
          <Button
            color="primary"
            variant="outlined"
            className={classes.cancelButton}
            data-testid="cancel-button"
            onClick={() => {
              resetForm();
              closeModal();
            }}
          >
            Cancel
          </Button>
          <LoadingButton
            key="Change-Pin-Save-Button"
            id="Change-Pin-Save-Button"
            type="submit"
            variant="contained"
            color="primary"
            className={classes.saveButton}
            loading={saveDisabled}
            text="Save"
          />
        </Box>
      </form>
    </Box>
  );
}

export default ChangePinForm;
