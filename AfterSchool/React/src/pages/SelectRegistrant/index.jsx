import { useSnackbar } from 'notistack';
import React, { useEffect } from 'react';
import { Box, Typography } from '@material-ui/core';

import utils from 'utils';
import useStyles from './styles';
import { useSelectRegistrant } from 'redux/hooks';
import { RegistrantCard, Loader } from 'components';

function SelectRegistrant() {
  const { redirect } = utils;
  const classes = useStyles();
  const { enqueueSnackbar } = useSnackbar();
  const { msgs, loading, students, loadData, addNewRegistrant } = useSelectRegistrant();

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  const handleClick = async ({ NewParticipant = false, BtnUrl }) => {
    if (NewParticipant) await addNewRegistrant();
    else redirect(BtnUrl);
  };

  if (loading) return <Loader />;

  return (
    <Box>
      <Typography className={classes.heading}>Select who you would like to register</Typography>
      <Box className={classes.outerWrapper}>
        <Box className={classes.innerWrapper}>
          {students.map((student, index) => (
            <RegistrantCard
              key={index}
              index={index}
              student={student}
              handleClick={() => handleClick(student)}
            />
          ))}
        </Box>
      </Box>
    </Box>
  );
}

export default SelectRegistrant;
