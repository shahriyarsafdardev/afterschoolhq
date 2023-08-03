import React, { useState } from 'react';
import { Backdrop, Box, Button, Fade, Typography, Modal } from '@material-ui/core';

import useStyles from './styles';
import ChangePinForm from './ChangePinForm';

function ChangePinCode() {
  const classes = useStyles();
  const [open, setOpen] = useState(false);
  const handleOpen = () => setOpen(true);
  const handleClose = () => setOpen(false);

  return (
    <Box>
      <Box className={classes.changePinBtnWrapper}>
        <Button
          color="primary"
          variant="contained"
          data-testid="change-pin-code-button"
          className={classes.changePinBtn}
          onClick={() => handleOpen()}
        >
          Change PIN Code
        </Button>
      </Box>
      <Modal
        aria-labelledby="transition-modal-title"
        aria-describedby="transition-modal-description"
        className={classes.modal}
        open={open}
        onClose={handleClose}
        closeAfterTransition
        BackdropComponent={Backdrop}
        BackdropProps={{
          timeout: 500
        }}
      >
        <Fade in={open}>
          <Box className={classes.paper}>
            <Typography variant="h4" className={classes.modalHeading}>
              Change PIN Code
            </Typography>

            <Box className={classes.formWrapper}>
              <Typography className={classes.modalDescription}>
                Please select four numbers to use as your PIN code when arriving to and departing
                from your activity.
              </Typography>
              <ChangePinForm closeModal={handleClose} />
            </Box>
          </Box>
        </Fade>
      </Modal>
    </Box>
  );
}

export default ChangePinCode;
