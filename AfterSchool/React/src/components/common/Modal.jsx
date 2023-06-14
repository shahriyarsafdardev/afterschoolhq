import React from 'react';
import { Dialog } from '@material-ui/core';

import useStyles from './styles';

const Modal = ({ isOpen, children, handleClose }) => {
  const classes = useStyles();

  return (
    <Dialog
      open={isOpen}
      onClose={handleClose}
      classes={{
        container: classes.modalRoot,
        paperWidthSm: classes.modal
      }}
    >
      {children}
    </Dialog>
  );
};

export default Modal;
