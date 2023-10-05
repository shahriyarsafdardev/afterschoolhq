import React from 'react';
import useStyles from '../Common/styles';
import { Box, Button, Grid, Typography } from '@material-ui/core';
import LoadingButton from 'components/Overrides/LoadingButton';
import ChildListRow from './ChildListRow';
import { useCheckInOut } from 'redux/hooks';

export default function ChildList({ setActiveStep, navigateToRoot }) {
  const classes = useStyles();
  const {
    childData: { AccountName, StudentsInfo = [] },
    selectedStudents
  } = useCheckInOut();

  return (
    <Box className={classes.childListForm}>
      <Box>
        <Typography className={classes.childListHeader}>{AccountName}</Typography>
        <Typography className={classes.subHeaderText}>Select all that apply.</Typography>
      </Box>
      <Box className={classes.childListWrapper}>
        {StudentsInfo.length > 0 ? (
          StudentsInfo.map((student, index) => <ChildListRow student={student} key={index} />)
        ) : (
          <Typography className={classes.subHeaderText}>Your access has been revoked!</Typography>
        )}
      </Box>
      <Grid item xs={12} className={classes.listButtonWrapper}>
        <Button
          type="submit"
          variant="outlined"
          color="primary"
          onClick={() => navigateToRoot()}
          fullWidth
          data-testid="cancel-button"
        >
          Cancel
        </Button>
        <LoadingButton
          type="submit"
          onClick={() => setActiveStep(2)}
          variant="contained"
          color="primary"
          fullWidth
          text="Next"
          disabled={!selectedStudents.length}
          loading={false}
        />
      </Grid>
    </Box>
  );
}
