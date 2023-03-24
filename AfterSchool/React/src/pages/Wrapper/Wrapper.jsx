import { Box } from '@material-ui/core';
import React, { useEffect } from 'react';
import { Outlet, useSearchParams } from 'react-router-dom';

import { NavBar, Loader } from 'components';
import { useUser, useRegistration } from 'redux/hooks/index';
import useStyles from './styles';

export default function Wrapper() {
  const classes = useStyles();
  const { loading, loadUser } = useUser();
  //   const { loadFormData } = useRegistration();
  const [searchParams, setSearchParams] = useSearchParams();

  useEffect(() => {
    loadUser();
    // loadFormData(searchParams.get('newChild'));
  }, []);

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
