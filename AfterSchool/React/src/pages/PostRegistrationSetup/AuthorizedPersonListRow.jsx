import { Checkbox, FormControlLabel, Box, Typography } from '@material-ui/core';
import TextField from 'components/Overrides/TextField';
import Controls from './Controls';
import useStyles from './styles';
import React, { useState } from 'react';
import { ErrorBand } from 'components';

function AuthorizedPersonListRow({
  handlePhoneInputChange,
  handleChange,
  touched,
  person,
  errors
}) {
  const classes = useStyles();
  const [phone, setPhone] = useState(Phone || null);
  const { FirstName, LastName, Phone, checked } = person;
  const [showPhoneField, setShowPhoneField] = useState(false);
  const toggleShowField = () => setShowPhoneField(!showPhoneField);

  const clearPhoneField = () => {
    toggleShowField();
    setPhone(Phone);
  };

  const savePhoneField = () => {
    toggleShowField();
    handlePhoneInputChange(phone);
  };

  const phoneField = () => (
    <Box className={classes.phoneField}>
      <TextField
        name={'Phone'}
        placeholder="Phone"
        variant="outlined"
        value={phone}
        onChange={e => setPhone(e.target.value)}
        required={true}
        type="number"
      />
      <Controls onClear={clearPhoneField} onSave={savePhoneField} />
    </Box>
  );

  return (
    <>
      <Box className={classes.listWrapper}>
        <Box className={classes.listInnerWrapper}>
          <Box>
            <FormControlLabel
              control={
                <Checkbox
                  checked={checked}
                  onChange={handleChange}
                  name={`${FirstName}_${LastName}`}
                />
              }
              label={`${FirstName} ${LastName}`}
            />
          </Box>
          {showPhoneField ? (
            phoneField()
          ) : (
            <Box className={classes.phoneField}>
              <Typography>{phone}</Typography>
              <Typography onClick={toggleShowField} className={classes.editButton}>
                Edit
              </Typography>
            </Box>
          )}
        </Box>
      </Box>
      {touched && errors && <ErrorBand error={errors} />}
    </>
  );
}

export default AuthorizedPersonListRow;
