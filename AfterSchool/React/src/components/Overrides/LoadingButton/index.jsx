import React from 'react';
import { Button, CircularProgress } from '@material-ui/core';

export default function index(props) {
  const { loading = false, text, disabled, ...restProps } = props;

  return (
    <Button disabled={loading || disabled} {...restProps} data-testid={`${text}-button`}>
      {loading ? (
        <>
          {'Loading . . .'} <CircularProgress size={'20px'} />
        </>
      ) : (
        text
      )}
    </Button>
  );
}
