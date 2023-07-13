import React from 'react';
import useStyles from '../Common/styles';
import { Box, Button, Grid, Typography } from '@material-ui/core';
import LoadingButton from 'components/Overrides/LoadingButton';
import ChildListRow from './ChildListRow';

export default function ChildList({ setSelectedChilds, StudentsInfo, AccountName }) {
  const classes = useStyles();
  const testData = [
    { name: 'Ahsan', activity: 'Oxford activity' },
    { name: 'Ellahi', activity: 'Martial Arts' },
    { name: 'Ahsan Ellahi', activity: 'Social work' }
  ];

  return (
    <Box className={classes.childListForm}>
      <Typography className={classes.childListHeader}>{AccountName}</Typography>
      <Typography className={classes.subHeaderText}>Select all that apply.</Typography>
      {StudentsInfo.map((student, index) => (
        <ChildListRow student={student} key={index} />
      ))}
      <Grid item xs={12} className={classes.listButtonWrapper}>
        <Button type="submit" variant="outlined" color="primary" fullWidth>
          Cancel
        </Button>
        <LoadingButton
          type="submit"
          onClick={() => setSelectedChilds(testData[0])}
          variant="contained"
          color="primary"
          fullWidth
          text="Next"
          loading={false}
        />
      </Grid>
    </Box>
  );
}
