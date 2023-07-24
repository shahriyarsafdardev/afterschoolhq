import { Box } from '@material-ui/core';
import React from 'react';
import { Done, CloseOutlined } from '@material-ui/icons';
import useStyles from './styles';

function Controls({ onSave, onClear }) {
  const classes = useStyles();
  return (
    <Box className={classes.controls}>
      <Done
        color="primary"
        data-testid="saveChanges"
        className={classes.cursorPointer}
        onClick={onSave}
        fontSize="large"
      />
      <CloseOutlined
        color="error"
        data-testid="closeField"
        onClick={onClear}
        className={classes.cursorPointer}
        fontSize="large"
      />
    </Box>
  );
}
export default Controls;
