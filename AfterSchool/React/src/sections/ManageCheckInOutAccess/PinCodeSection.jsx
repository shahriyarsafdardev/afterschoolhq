import React from 'react';
import { Box, Divider, Typography } from '@material-ui/core';

import useStyles from './styles';
import PinSetupForm from './PinSetupForm';
import ChangePinCode from './ChangePinCode';

function PinCodeSection(props) {
  const classes = useStyles();
  const { IsPinSetup, IsPhoneSetup } = props;

  return (
    <Box>
      <Typography variant="h6" className={classes.sectionName}>
        Set PIN Code
      </Typography>
      <Typography className={classes.heading}>
        Please select four numbers to use as your PIN code when arriving to and departing from your
        activity.
      </Typography>
      <Divider className={classes.divider} />
      {!IsPinSetup ? <PinSetupForm isPhoneSetup={IsPhoneSetup} /> : <ChangePinCode />}
    </Box>
  );
}

export default PinCodeSection;
