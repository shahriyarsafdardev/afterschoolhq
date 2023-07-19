import React, { useEffect, useState } from 'react';
import useStyles from '../Common/styles';
import { useSnackbar } from 'notistack';
import { Box, Card, CircularProgress, Typography, Grid, Button } from '@material-ui/core';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import PinInputField from '../Common/PinInputField';

function index() {
  const classes = useStyles();
  const [pin, setPin] = useState('');
  const [userPin, setUserPin] = useState();
  const [searchParams, setSearchParams] = useSearchParams();
  const phone = searchParams.get('phone') || '';
  const handleSubmit = () => {
    setUserPin(pin);
  };
  return (
    <Grid container className={classes.wrapper}>
      <Card className={classes.innerContainer}>
        <Box className={classes.content}>
          <Box className={classes.pinSetupForm}>
            <Typography className={classes.headerText}>Reset your PIN code.</Typography>
            <Typography className={classes.subHeaderText}>
              Please select four numbers to use for your PIN code during check in and check out.
            </Typography>
            <PinInputField handleChange={setPin} errorMessage={''} />
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
        </Box>
      </Card>
    </Grid>
  );
}

export default index;
