import React, { useEffect } from 'react';
import { makeStyles } from '@material-ui/core';
import { SnackbarProvider as NotistackSnackbarProvider, useSnackbar } from 'notistack';

import { useErrorMessages } from 'redux/hooks';

const ErrorSnackbarProvider = ({ children }) => {
  const { msgs } = useErrorMessages();
  const { enqueueSnackbar } = useSnackbar();

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  return <div>{children}</div>;
};

function SnackbarProvider({ children }) {
  const classes = makeStyles(theme => ({
    variantSuccess: { backgroundColor: `${theme.palette.primary.main} !important` }
  }))();

  return (
    <NotistackSnackbarProvider
      maxSnack={3}
      anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
      classes={{ variantSuccess: classes.variantSuccess }}
    >
      <ErrorSnackbarProvider>{children}</ErrorSnackbarProvider>
    </NotistackSnackbarProvider>
  );
}

export default SnackbarProvider;
