import React from 'react';
import { Box } from '@material-ui/core';
import { useSnackbar } from 'notistack';

function CopyClipBoard(props) {
  const { enqueueSnackbar } = useSnackbar();
  const { children: CopyButton, textToCopy } = props;

  const copyToClipBoard = async copyMe => {
    try {
      await navigator.clipboard.writeText(copyMe);
      enqueueSnackbar('URL has been Copied!', { variant: 'success' });
    } catch (err) {
      enqueueSnackbar('Failed to copy!', { variant: 'error' });
    }
  };

  return (
    <Box
      onClick={() => {
        copyToClipBoard(textToCopy);
      }}
    >
      {CopyButton}
    </Box>
  );
}

export default CopyClipBoard;
