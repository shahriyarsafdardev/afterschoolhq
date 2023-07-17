import { Box, Typography } from '@material-ui/core';
import { isEmpty } from 'lodash';
import React from 'react';

export default function Confirmation({ checkedInStudents, checkedOutStudents }) {
  return (
    <Box>
      {!isEmpty(checkedInStudents) && (
        <Typography variant="body1">I am checking in {checkedInStudents}</Typography>
      )}
      {!isEmpty(checkedOutStudents) && (
        <Typography variant="body1">I am checking out {checkedOutStudents}</Typography>
      )}
    </Box>
  );
}
