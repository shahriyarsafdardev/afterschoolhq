import { Box } from '@material-ui/core';
import React, { useEffect } from 'react';
import { Outlet } from 'react-router-dom';

import { NavBar, Loader } from 'components';
import { useUser } from 'redux/hooks/index';
import useStyles from './styles';

export default function Wrapper() {
  const classes = useStyles();
  const { loading, loadUser } = useUser();

  useEffect(() => loadUser(), []);

  return (
    <Box className={classes.home}>
      {loading ? (
        <Loader />
      ) : (
        <>
          <NavBar />
          <Outlet />
        </>
      )}
    </Box>
  );
}
