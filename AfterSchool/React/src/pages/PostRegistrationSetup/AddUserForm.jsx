import { Box, Checkbox, Grid, Typography } from '@material-ui/core';
import React from 'react';

import TextField from 'components/Overrides/TextField';

function AddUserForm({ values, handleInputChange, touched, errors }) {
  const formField = ({ name, label, value }) => (
    <>
      <TextField
        name={name}
        value={value}
        onChange={e => handleInputChange(e)}
        fullWidth
        error={!!(touched[name] && errors[name])}
        helperText={touched[name] && errors[name]}
      />
      <Typography>{label}</Typography>
    </>
  );

  return (
    <Box style={{ display: 'flex', alignItems: 'baseline' }}>
      <Box style={{ paddingTop: '8' }}>
        <Checkbox checked={values.checked} onChange={e => handleInputChange(e)} name="checked" />
      </Box>
      <Grid container spacing={2} style={{ textAlign: 'left' }}>
        <Grid item xs={4}>
          {formField({ name: 'FirstName', label: 'First Name', value: values.firstName })}
        </Grid>
        <Grid item xs={4}>
          {formField({ name: 'LastName', label: 'Last Name', value: values.lastName })}
        </Grid>
        <Grid item xs={4}>
          {formField({ name: 'Phone', label: 'Phone Number', value: values.phoneNumber })}
        </Grid>
      </Grid>
    </Box>
  );
}

export default AddUserForm;
