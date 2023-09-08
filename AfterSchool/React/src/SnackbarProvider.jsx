import React from 'react';
import { makeStyles } from '@material-ui/core';
import CloseIcon from '@material-ui/icons/Close';
import { SnackbarProvider as NotistackSnackbarProvider, useSnackbar } from 'notistack';

function SnackbarCloseButton({ snackbarKey }) {
  const { closeSnackbar } = useSnackbar();

  return (
    <CloseIcon
      style={{ cursor: 'pointer', fontSize: 'small' }}
      onClick={() => closeSnackbar(snackbarKey)}
    />
  );
}

function SnackbarProvider({ children }) {
  const classes = makeStyles(theme => ({
    variantSuccess: { backgroundColor: `${theme.palette.primary.main} !important` }
  }))();

  return (
    <NotistackSnackbarProvider
      maxSnack={3}
      anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
      classes={{ variantSuccess: classes.variantSuccess }}
      action={snackbarKey => <SnackbarCloseButton snackbarKey={snackbarKey} />}
    >
      {children}
    </NotistackSnackbarProvider>
  );
}

export default SnackbarProvider;
