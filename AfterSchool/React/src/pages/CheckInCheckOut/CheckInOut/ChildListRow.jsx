import { Box, Typography } from '@material-ui/core';
import React from 'react';
import useStyles from '../Common/styles';
import ActivityRow from './ActivityRow';

export default function ChildListRow({ student }) {
  const classes = useStyles();
  const { Activities } = student;
  return (
    <Box>
      <Typography className={classes.fontBold}>{student?.StudentName}</Typography>
      {Activities &&
        Activities.map((activity, index) => {
          return <ActivityRow activity={activity} key={index} />;
        })}
    </Box>
  );
}
