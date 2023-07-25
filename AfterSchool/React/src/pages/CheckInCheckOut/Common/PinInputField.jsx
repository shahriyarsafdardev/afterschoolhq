import React from 'react';
import useStyles from './styles';
import { Box } from '@material-ui/core';
import PinInput from 'react-pin-input';
import { ErrorBand } from 'components';

export default function PinInputField(props) {
  const { errorMessage, handleChange } = props;
  const classes = useStyles();

  return (
    <Box className={classes.marginAuto}>
      <PinInput
        length={4}
        onChange={value => {
          handleChange(value);
        }}
        type="numeric"
        inputMode="number"
        data-testid="pin-field"
        className={classes.pinField}
        regexCriteria={/^[ A-Za-z0-9_@./#&+-]*$/}
        inputStyle={{ border: '3px solid #E4E4E4', borderRadius: '6px' }}
      />
      {errorMessage && <ErrorBand error={errorMessage} />}
    </Box>
  );
}
