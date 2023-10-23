import React, { useEffect } from 'react';
import { Box, Typography } from '@material-ui/core';

import utils from 'utils';
import useStyles from './styles';
import { useSelectRegistrant } from 'redux/hooks';
import { AddNewParticipantCard, RegistrantCard, Loader } from 'components';

function SelectRegistrant() {
  const { redirect } = utils;
  const classes = useStyles();
  const { loading, students, loadData } = useSelectRegistrant();

  useEffect(() => {
    loadData();
  }, []);

  const handleClick = async ({ BtnUrl }) => redirect(BtnUrl);

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
          <AddNewParticipantCard />
        </Box>
      </Box>
    </Box>
  );
}

export default SelectRegistrant;
