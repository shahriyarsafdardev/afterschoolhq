import { Box, Typography } from '@material-ui/core';
import React from 'react';
import useStyles from '../Common/styles';
import ActivityRow from './ActivityRow';
import { useCheckInOut } from 'redux/hooks';

export default function ChildListRow({ student }) {
  const classes = useStyles();
  const { Activities, StudentName } = student;
  const { updateSelectedStudentsData, selectedStudents } = useCheckInOut();

  const onSelect = (ActivityUid, CheckIn, StudentUid) => {
    let updatedStudentsList = selectedStudents;
    const studentMatch = ({ StudentUid: studentId, ActivityUid: activityId }) =>
      studentId === StudentUid && activityId === ActivityUid;

    const alreadySelected = selectedStudents.find(std => studentMatch(std));

    if (alreadySelected)
      updatedStudentsList = updatedStudentsList.filter(std => !studentMatch(std));
    else
      updatedStudentsList = [
        ...updatedStudentsList,
        { StudentUid, ActivityUid, CheckInOut: !CheckIn, StudentName }
      ];

    updateSelectedStudentsData(updatedStudentsList);
  };

  return (
    <Box className={classes.widthFull}>
      <Typography className={classes.fontBold}>{student?.StudentName}</Typography>
      {Activities &&
        Activities.map((activity, index) => {
          return <ActivityRow activity={activity} key={index} onSelect={onSelect} />;
        })}
    </Box>
  );
}
