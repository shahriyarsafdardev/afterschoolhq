import { Box, Typography } from '@material-ui/core';
import React from 'react';
import SignatureCanvas from 'react-signature-canvas';
import useStyles from '../Common/styles';

export default function EsignPage() {
  const classes = useStyles();
  return (
    <Box className={classes.esignPage}>
      <Typography className={classes.headerText}>Please sign below.</Typography>
      <SignatureCanvas
        penColor="black"
        backgroundColor="#B2B2B4"
        canvasProps={{ width: 400, height: 200 }}
      />
    </Box>
  );
}
