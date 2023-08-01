import { Box, Button, Grid, Typography, useMediaQuery, useTheme } from '@material-ui/core';
import React, { useState } from 'react';
import useStyles from '../Common/styles';
import LoadingButton from 'components/Overrides/LoadingButton';
import EsignComponent from './EsignComponent';
import Confirmation from './Confirmation';
import { useCheckInOut } from 'redux/hooks';
import { join, uniq } from 'lodash';

export default function EsignPage({ setSignature, setActiveStep }) {
  const theme = useTheme();
  const classes = useStyles();
  const [eSign, setEsign] = useState(null);
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const { selectedStudents } = useCheckInOut();

  const checkedInStudents = selectedStudents
    .filter(std => std.CheckInOut)
    .map(std => std.StudentName);

  const checkedOutStudents = selectedStudents
    .filter(std => !std.CheckInOut)
    .map(std => std.StudentName);

  function formatNames(names) {
    if (!Array.isArray(names) || names.length === 0) {
      return '';
    }

    if (names.length <= 2) return join(uniq(names), ' and ');

    const lastStudentName = names.pop();
    return join(uniq(names), ', ') + ' and ' + lastStudentName;
  }

  const formattedCheckedIn = formatNames(checkedInStudents);
  const formattedCheckedOut = formatNames(checkedOutStudents);

  return (
    <Box className={classes.esignPage}>
      <Typography className={classes.font18Bold}>Please sign below.</Typography>
      <Confirmation
        checkedInStudents={formattedCheckedIn}
        checkedOutStudents={formattedCheckedOut}
      />
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
          data-testid="back-button"
        >
          Back
        </Button>
        <LoadingButton
          onClick={() => setSignature(eSign, formattedCheckedIn, formattedCheckedOut)}
          variant="contained"
          color="primary"
          fullWidth
          text="Complete"
          loading={false}
          disabled={!eSign}
        />
      </Grid>
    </Box>
  );
}
