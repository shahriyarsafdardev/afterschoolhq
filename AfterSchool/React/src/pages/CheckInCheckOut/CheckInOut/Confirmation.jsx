import { Box, Typography } from '@material-ui/core';
import { join, uniq, isEmpty } from 'lodash';
import React from 'react';
import { useCheckInOut } from 'redux/hooks';

export default function Confirmation() {
  const { selectedStudents } = useCheckInOut();

  const checkedInStudents = selectedStudents
    .filter(std => !std.checkInOut)
    .map(std => std.StudentName);
  const checkedOutStudents = selectedStudents
    .filter(std => std.checkInOut)
    .map(std => std.StudentName);

  function formatNames(names) {
    if (!Array.isArray(names) || names.length === 0) {
      return '';
    }

    if (names.length <= 2) return join(uniq(names), ' and ');

    const lastStudentName = names.pop();
    return join(uniq(names), ', ') + ' and ' + lastStudentName;
  }

  return (
    <Box>
      {!isEmpty(checkedInStudents) && (
        <Typography variant="body1">I am checking in {formatNames(checkedInStudents)}</Typography>
      )}
      {!isEmpty(checkedOutStudents) && (
        <Typography variant="body1">I am checking out {formatNames(checkedOutStudents)}</Typography>
      )}
    </Box>
  );
}
