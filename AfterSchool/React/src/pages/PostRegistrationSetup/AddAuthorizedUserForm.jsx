import { Box, Checkbox, Grid, Typography } from '@material-ui/core';
import React, { useState } from 'react';

import TextField from 'components/Overrides/TextField';
import Controls from './Controls';
import useStyles from './styles';

function AddAuthorizedUserForm({ setFieldValue, values: user }) {
  const classes = useStyles();

  const [values, setValue] = useState(user || {});

  const handleChange = ({ target: { name, value } }) => {
    setValue({ ...values, [name]: name === 'checked' ? !values.checked : value });
  };

  const formField = ({ name, label, value }) => (
    <>
      <TextField
        name={name}
        value={value}
        onChange={handleChange}
        fullWidth
        type={name === 'Phone' ? 'number' : 'text'}
      />
      <Typography>{label}</Typography>
    </>
  );

  return (
    <Box className={classes.fieldWrapper}>
      <Box style={{}}>
        <Checkbox
          className={classes.checkboxPadding}
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
              setValue(user);
            }}
            onSave={() => setFieldValue({ ...values, default: true })}
          />
        </Grid>
      </Grid>
    </Box>
  );
}

export default AddAuthorizedUserForm;
