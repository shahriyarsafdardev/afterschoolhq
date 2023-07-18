import React from 'react';
import useStyles from '../Common/styles';
import CircleChecked from '@material-ui/icons/CheckCircleOutline';
import CircleUnchecked from '@material-ui/icons/RadioButtonUnchecked';
import { Box, Typography, Checkbox, FormControlLabel, Grid } from '@material-ui/core';
import { useCheckInOut } from 'redux/hooks';

export default function ActivityRow({ activity, onSelect, checked }) {
  const classes = useStyles();
  const { selectedStudents } = useCheckInOut();

  const {
    ActivityEndTime,
    ActivityName,
    ActivityStartTime,
    ActivityUid,
    StudentUid,
    CheckSetupDto
  } = activity;

  const { CheckIn = false, CheckInTime, CheckInBy } = CheckSetupDto || {};

  const studentMatch = ({ StudentUid: studentId, ActivityUid: activityId }) =>
    studentId === StudentUid && activityId === ActivityUid;

  const alreadySelected = selectedStudents.find(std => studentMatch(std));

  const handleSelect = () => {
    onSelect(ActivityUid, CheckIn, StudentUid);
  };

  const isChecked = checked || Boolean(alreadySelected);
  const label = CheckIn ? 'Check Out' : 'Check In';
  return (
    <Box>
      <Grid container className={classes.activityRow}>
        <Grid item lg={9}>
          <Typography className={classes.fontLight}>
            {ActivityName}{' '}
            {ActivityStartTime && (
              <span>
                : {ActivityStartTime} - {ActivityEndTime}
              </span>
            )}
          </Typography>
        </Grid>
        <Grid item lg={3} className={classes.displayContents}>
          <FormControlLabel
            control={
              <Checkbox
                icon={<CircleUnchecked />}
                checkedIcon={<CircleChecked />}
                label="Check"
                checked={isChecked}
                classes={{
                  checked: classes.checked,
                  root: classes.root
                }}
              />
            }
            classes={{
              root: classes.formControlLabelRoot
            }}
            label={label}
            onChange={handleSelect}
          />
        </Grid>
      </Grid>
      {CheckIn && (
        <Typography variant="body2" className={classes.checkedInMsg}>
          Checked in at {CheckInTime} by {CheckInBy}
        </Typography>
      )}
    </Box>
  );
}
