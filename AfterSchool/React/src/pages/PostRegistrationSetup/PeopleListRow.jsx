import { Checkbox, FormControlLabel, Box } from '@material-ui/core';
import TextField from 'components/Overrides/TextField';
import React from 'react';

function PeopleListRow({ person, handleChange, handlePhoneInputChange, touched, errors }) {
  const { FirstName, LastName, Phone, checked } = person;
  return (
    <>
      <Box style={{ display: 'flex', flexDirection: 'column', width: '100%' }}>
        <Box style={{ display: 'flex', alignItems: 'baseline' }}>
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
          {!Phone && (
            <TextField
              name={'Phone'}
              placeholder="Phone"
              variant="standard"
              onChange={e => handlePhoneInputChange(e)}
              error={!!(touched['Phone'] && errors['Phone'])}
              helperText={touched['Phone'] && errors['Phone']}
              required={true}
              type="number"
            />
          )}
        </Box>
      </Box>
    </>
  );
}

export default PeopleListRow;
