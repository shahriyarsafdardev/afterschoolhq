import { Box, Checkbox, Grid, Typography } from '@material-ui/core';
import CheckIcon from '@material-ui/icons/Check';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import React from 'react';

import TextField from 'components/Overrides/TextField';
import { phoneValidationRegex } from 'constants/regex';
import Controls from './Controls';
import useStyles from './styles';

function AddAuthorizedUserForm({ setFieldValue, person: user }) {
  const classes = useStyles();

  const handleChange = () => {
    setFieldValue({ ...user, checked: !user.checked });
    setValue('checked', !user.checked);
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
      setFieldValue({ ...values, default: true });
    }
  });

  const { values, getFieldProps, touched, errors, handleSubmit, setFieldValue: setValue } = formik;

  const formField = ({ name, label, value }) => (
    <>
      <TextField
        name={name}
        value={value}
        fullWidth
        {...getFieldProps(name)}
        type={name === 'Phone' ? 'number' : 'text'}
        error={!!(touched && touched[name] && errors[name])}
        helperText={touched && touched[name] && errors[name]}
      />
      <Typography>{label}</Typography>
    </>
  );

  return (
    <Box className={classes.fieldWrapper}>
      <Box style={{ marginLeft: '-11px' }}>
        <Checkbox
          classes={{
            checked: classes.checked,
            root: classes.root
          }}
          checkedIcon={<CheckIcon className={classes.checkBoxIcon} />}
          iconstyle={{ fill: 'black' }}
          checked={values.checked}
          onChange={handleChange}
          name="checked"
        />
      </Box>
      <Grid container spacing={2} className={classes.alignLeft}>
        <Grid item xs={3}>
          {formField({ name: 'FirstName', label: 'First Name', value: values.FirstName })}
        </Grid>
        <Grid item xs={3}>
          {formField({ name: 'LastName', label: 'Last Name', value: values.LastName })}
        </Grid>
        <Grid item xs={4}>
          {formField({ name: 'Phone', label: 'Phone Number', value: values.Phone })}
        </Grid>
        <Grid item xs={2} style={{ margin: 'auto' }}>
          <Controls
            onClear={() => {
              setFieldValue({ ...user, default: true });
            }}
            onSave={handleSubmit}
          />
        </Grid>
      </Grid>
    </Box>
  );
}

export default AddAuthorizedUserForm;
