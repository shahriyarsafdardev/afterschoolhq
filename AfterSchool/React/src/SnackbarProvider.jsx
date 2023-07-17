import React from 'react';
import { SnackbarProvider as NotistackSnackbarProvider } from 'notistack';
import { makeStyles } from '@material-ui/core';

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
      {children}
    </NotistackSnackbarProvider>
  );
}

export default SnackbarProvider;
