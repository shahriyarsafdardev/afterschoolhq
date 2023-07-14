import React from 'react';
import useStyles from '../Common/styles';
import { Box, Typography, Checkbox, FormControlLabel, useTheme } from '@material-ui/core';
import CircleChecked from '@material-ui/icons/CheckCircleOutline';
import CircleUnchecked from '@material-ui/icons/RadioButtonUnchecked';

export default function ActivityRow({ activity }) {
  const classes = useStyles();

  const handleSelect = () => {};

  const { ActivityEndTime, ActivityName, ActivityStartTime, ActivityUid } = activity;
  return (
    <Box className={classes.activityRow}>
      <Typography className={classes.fontLight}>
        {ActivityName}{' '}
        {ActivityStartTime && (
          <span>
            : {ActivityStartTime} - {ActivityEndTime}
          </span>
        )}
      </Typography>
      <FormControlLabel
        control={
          <Checkbox
            icon={<CircleUnchecked />}
            checkedIcon={<CircleChecked />}
            label="Check"
            classes={{
              checked: classes.checked,
              root: classes.root
            }}
          />
        }
        classes={{
          root: classes.formControlLabelRoot
        }}
        label="Check"
        onChange={handleSelect}
      />
    </Box>
  );
}
