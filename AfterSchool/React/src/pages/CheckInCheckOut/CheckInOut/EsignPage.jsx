import { Box, Button, Grid, Typography, useMediaQuery, useTheme } from '@material-ui/core';
import React, { useState } from 'react';
import useStyles from '../Common/styles';
import LoadingButton from 'components/Overrides/LoadingButton';
import EsignComponent from './EsignComponent';
import Confirmation from './Confirmation';

export default function EsignPage({ setSignature, setActiveStep }) {
  const theme = useTheme();
  const classes = useStyles();
  const [eSign, setEsign] = useState(null);
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));

  return (
    <Box className={classes.esignPage}>
      <Typography className={classes.headerText}>Please sign below.</Typography>
      <Confirmation />
      {isMobile ? (
        <EsignComponent handleSubmit={setEsign} type="pad" />
      ) : (
        <EsignComponent handleSubmit={setEsign} type="text" value={eSign} />
      )}
      <Grid item xs={12} className={classes.listButtonWrapper}>
        <Button
          variant="outlined"
          color="primary"
          fullWidth
          onClick={() => {
            setActiveStep(1);
          }}
        >
          Back
        </Button>
        <LoadingButton
          onClick={() => setSignature(eSign)}
          variant="contained"
          color="primary"
          fullWidth
          text="Next"
          loading={false}
          disabled={!eSign}
        />
      </Grid>
    </Box>
  );
}
