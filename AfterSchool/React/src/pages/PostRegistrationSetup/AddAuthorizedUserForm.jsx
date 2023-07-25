import { Box, Checkbox, Grid, Typography } from '@material-ui/core';
import CheckIcon from '@material-ui/icons/Check';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import React from 'react';

import TextField from 'components/Overrides/TextField';
import { phoneValidationRegex } from 'constants/regex';
import Controls from './Controls';
import useStyles from './styles';

function AddAuthorizedUserForm({ setFieldValue, handleDeleteField, person: user }) {
  const classes = useStyles();

  const handleChange = () => {
    setFieldValue({ ...user, HasAccess: !user.HasAccess });
    setValue('HasAccess', !user.HasAccess);
  };

  const ValidationSchema = Yup.object().shape({
    Phone: Yup.string()
      .matches(phoneValidationRegex, 'Phone is not valid')
      .required('Phone is required'),
    FirstName: Yup.string().required('First Name is required'),
    LastName: Yup.string().required('Last Name is required')
  });

  const formik = useFormik({
    initialValues: user,
    validationSchema: ValidationSchema,
    onSubmit: () => {
      setFieldValue({ ...values, default: true, newObject: false });
    }
  });

  const { values, getFieldProps, touched, errors, handleSubmit, setFieldValue: setValue } = formik;
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
      <Box style={{ marginLeft: '-11px' }}>
        <Checkbox
          classes={{
            checked: classes.checked,
            root: classes.root
          }}
          name="checked"
          onChange={handleChange}
          checked={values.HasAccess}
          iconstyle={{ fill: 'black' }}
          data-testid={`${values.FirstName}-checked`}
          checkedIcon={<CheckIcon className={classes.checkBoxIcon} />}
        />
      </Box>
      <Grid container spacing={2} className={classes.alignLeft}>
        <Grid item sm={10} xs={12} md={3}>
          {formField({ name: 'FirstName', label: 'First Name', value: values.FirstName })}
        </Grid>
        <Grid item sm={10} xs={12} md={3}>
          {formField({ name: 'LastName', label: 'Last Name', value: values.LastName })}
        </Grid>
        <Grid item sm={10} xs={12} md={4}>
          {formField({ name: 'Phone', label: 'Phone Number', value: values.Phone })}
        </Grid>
        <Grid item sm={10} xs={12} md={2} style={{ margin: 'auto' }}>
          <Controls onClear={onClear} onSave={handleSubmit} />
        </Grid>
      </Grid>
    </Box>
  );
}

export default AddAuthorizedUserForm;
