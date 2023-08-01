import { Box, Typography } from '@material-ui/core';
import { isEmpty } from 'lodash';
import React from 'react';
import useStyles from '../Common/styles';

export default function Confirmation({ checkedInStudents, checkedOutStudents }) {
  const classes = useStyles();
  return (
    <Box>
      {!isEmpty(checkedInStudents) && (
        <Typography variant="body1" className={classes.font16Light}>
          I am checking in {checkedInStudents}
        </Typography>
      )}
      {!isEmpty(checkedOutStudents) && (
        <Typography variant="body1" className={classes.font16Light}>
          I am checking out {checkedOutStudents}
        </Typography>
      )}
    </Box>
  );
}
