import { Box, Button, Typography } from '@material-ui/core';
import React, { useState } from 'react';
import useStyles from './styles';
import PinInput from 'react-pin-input';

function PinSetupForm({ setUserPin, currentUserInfo }) {
  const classes = useStyles();
  const [pin, setPin] = useState('');
  const { StudentFirstName, StudentLastName, ParentFirstName, ParentLastName } = currentUserInfo;

  const handleSubmit = () => {
    setUserPin(pin);
  };

  return (
    <Box className={classes.pinSetupForm}>
      <Typography className={classes.headerText}>
        {StudentFirstName} {StudentLastName} has registered for an activity that requires an
        authorized person to check them in and out. You have been invited by {ParentFirstName}{' '}
        {ParentLastName} to be an authorized person for {StudentFirstName}.
      </Typography>
      <Typography className={classes.subHeaderText}>
        Please select four numbers to use for your PIN code during check in and check out.
      </Typography>
      <Box className={classes.marginAuto}>
        <PinInput
          length={4}
          onChange={value => {
            setPin(value);
          }}
          type="numeric"
          inputMode="number"
          style={{ padding: '10px' }}
          inputStyle={{ border: '3px solid #E4E4E4', borderRadius: '6px' }}
          regexCriteria={/^[ A-Za-z0-9_@./#&+-]*$/}
        />
      </Box>
      <Button
        onClick={handleSubmit}
        className={classes.buttonNext}
        variant="contained"
        color="primary"
        disabled={pin.length !== 4}
      >
        Next
      </Button>
    </Box>
  );
}

export default PinSetupForm;
