import React from 'react';
import { Button, CircularProgress } from '@material-ui/core';

export default function index(props) {
  const { loading = false, text, disabled = false, ...restProps } = props;

  return (
    <Button disabled={loading || disabled} {...restProps} data-testid={`${text}-button`}>
      {text}
      {loading && <CircularProgress size={'20px'} style={{ marginLeft: 5 }} />}
    </Button>
  );
}
