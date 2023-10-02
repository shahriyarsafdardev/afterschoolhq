import { Box, Grid, Typography } from '@material-ui/core';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import React from 'react';

import TextField from 'components/Overrides/TextField';
import { phoneValidationRegex } from 'constants/regex';
import { withoutSignsPhoneNumber } from 'utils';
import Controls from './Controls';
import useStyles from './styles';

function AddAuthorizedUserForm({
  setFieldValue,
  handleDeleteField,
  person: user,
  authorizedPersonsPhoneList
}) {
  const classes = useStyles();

  const ValidationSchema = Yup.object().shape({
    Phone: Yup.string()
      .matches(phoneValidationRegex, 'Phone is not valid')
      .test('unique-phone', 'Phone already exists', function(value) {
        const phoneWithoutSign = withoutSignsPhoneNumber(value);
        return (
          !authorizedPersonsPhoneList.includes(phoneWithoutSign) || user.Phone === phoneWithoutSign
        );
      })
      .required('Phone is required'),
    FirstName: Yup.string().required('First Name is required'),
    LastName: Yup.string().required('Last Name is required')
  });

  const formik = useFormik({
    initialValues: user,
    validationSchema: ValidationSchema,
    onSubmit: () => {
      const updatedValues = {
        ...values,
        default: true,
        newObject: false,
        Phone: withoutSignsPhoneNumber(values.Phone)
      };
      setFieldValue(updatedValues);
    }
  });

  const { values, getFieldProps, touched, errors, handleSubmit } = formik;

  const onClear = () => {
    setFieldValue({ ...user, default: true });
    const { FirstName, LastName, Phone, newObject } = user;
    if (!(FirstName || LastName || Phone) && newObject) handleDeleteField();
  };

  const formField = ({ name, label, value }) => (
    <Box fullWidth>
      <TextField
        name={name}
        value={value}
        fullWidth
        {...getFieldProps(name)}
        data-testid={`${name}-add-user-field`}
        type="text"
        error={!!(touched && touched[name] && errors[name])}
        helperText={touched && touched[name] && errors[name]}
      />
      <Typography>{label}</Typography>
    </Box>
  );

  return (
    <Box className={classes.fieldWrapper}>
      <Grid container spacing={2} className={classes.alignLeft}>
        <Grid item sm={10} xs={12} md={3}>
          {formField({ name: 'FirstName', label: 'First Name', value: values.FirstName })}
        </Grid>
        <Grid item sm={10} xs={12} md={3}>
          {formField({ name: 'LastName', label: 'Last Name', value: values.LastName })}
        </Grid>
        <Grid item sm={10} xs={12} md={3}>
          {formField({ name: 'Phone', label: 'Phone Number', value: values.Phone })}
        </Grid>
        <Grid item sm={10} xs={12} md={3} style={{ margin: 'auto' }}>
          <Controls onClear={onClear} onSave={handleSubmit} />
        </Grid>
      </Grid>
    </Box>
  );
}

export default AddAuthorizedUserForm;
