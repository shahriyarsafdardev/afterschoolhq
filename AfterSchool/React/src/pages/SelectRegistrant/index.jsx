import React from 'react';
import { Box, Typography } from '@material-ui/core';
import { RegistrantCard } from 'components';
import useStyles from './styles';

function SelectRegistrant() {
  const classes = useStyles();
  const keys = [...Array(3).keys()];

  return (
    <Box>
      <Typography className={classes.heading}>Select who you would like to register</Typography>
      <Box className={classes.outerWrapper}>
        <Box className={classes.innerWrapper}>
          {keys.map(item => (
            <RegistrantCard key={item} name={'Ahsan Ellahi'} showMsg={true} />
          ))}
          <RegistrantCard name={'Ahsan Ellahi'} showMsg={true} />
          <RegistrantCard name={'Ahsan Ellahi'} showMsg={false} />
          <RegistrantCard name={null} showMsg={false} />
        </Box>
      </Box>
    </Box>
  );
}

export default SelectRegistrant;
