import * as Yup from 'yup';
import { useFormik } from 'formik';
import CloseIcon from '@material-ui/icons/Close';
import React, { useEffect, useState } from 'react';
import { Backdrop, Box, Fade, Typography, Modal, Button, Grid, Divider } from '@material-ui/core';

import useStyles from './styles';
import { useSnackbar } from 'notistack';
import { useParams } from 'react-router-dom';
import { useRegistrantNotes } from 'redux/hooks';
import TextField from 'components/Overrides/TextField';
import LoadingButton from 'components/Overrides/LoadingButton';
import TextAreaField from 'components/Overrides/TextAreaField';
import { changeDateTimeFormat, getCurrentDateTime } from 'constants';

function AddNewNoteModal(props) {
  const classes = useStyles();
  const { open, setOpen } = props;
  const { enqueueSnackbar } = useSnackbar();

  const handleClose = () => setOpen(false);
  const { regUid } = useParams();
  const [saveDisabled, setSaveDisabled] = useState(false);

  const { createNote, msgs, registrantNotes } = useRegistrantNotes();

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  useEffect(() => {
    setFieldValue('postDate', getCurrentDateTime());
  }, []);

  const ValidationSchema = Yup.object().shape({
    title: Yup.string().required('Title is required'),
    text: Yup.string()
      .required('Text is required')
      .max(4000, 'Text should be less than 4000 characters'),
    postDate: Yup.date()
      .required('Date is required')
      .max(new Date(getCurrentDateTime()), `Date should be not be in future`)
  });

  const formik = useFormik({
    initialValues: {
      title: '',
      text: '',
      postDate: getCurrentDateTime()
    },
    validationSchema: ValidationSchema,
    onSubmit: async (values, { resetForm }) => {
      setSaveDisabled(true);
      const formattedDate = changeDateTimeFormat(values.postDate);
      await createNote({
        ...values,
        postDate: formattedDate,
        registrationUid: regUid,
        registrantNotes: registrantNotes
      }).then(msgs => {
        if (!msgs.length) {
          enqueueSnackbar('Submitted.', { variant: 'success' });
          resetForm();
          handleClose();
        }
        setSaveDisabled(false);
      });
    }
  });

  const { errors, touched, handleSubmit, getFieldProps, setFieldValue, resetForm } = formik;

  const submitNote = e => {
    e.preventDefault();
    handleSubmit();
  };

  const closeModal = () => {
    resetForm();
    handleClose();
  };

  return (
    <Modal
      aria-labelledby="transition-modal-title"
      aria-describedby="transition-modal-description"
      className={classes.modal}
      open={open}
      onClose={closeModal}
      closeAfterTransition
      BackdropComponent={Backdrop}
      BackdropProps={{
        timeout: 500
      }}
    >
      <Fade in={open}>
        <Box className={classes.paper}>
          <Box className={classes.modalHeading}>
            <Typography variant="h4" className={classes.headerText}>
              Add New Note
            </Typography>
            <CloseIcon onClick={closeModal} className={classes.closeIcon} />
          </Box>
          <Box className={classes.formWrapper}>
            <Grid container className={classes.gridWrapper}>
              <Grid item xs={12} md={7}>
                <TextField
                  id="title"
                  data-testid="title-field"
                  customLabel="Title"
                  variant="outlined"
                  autofill="false"
                  required={true}
                  fullWidth
                  {...getFieldProps('title')}
                  error={!!(touched['title'] && errors['title'])}
                  helperText={touched['title'] && errors['title']}
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <TextField
                  id="datetime-local"
                  data-testid="date-field"
                  customLabel="Date"
                  variant="outlined"
                  type="datetime-local"
                  required={true}
                  {...getFieldProps('postDate')}
                  InputLabelProps={{
                    shrink: true
                  }}
                  error={!!(touched['postDate'] && errors['postDate'])}
                  helperText={touched['postDate'] && errors['postDate']}
                />
              </Grid>
              <Grid item xs={12}>
                <TextAreaField
                  data-testid="text-field"
                  customLabel="Note"
                  variant="outlined"
                  autofill="false"
                  required={true}
                  {...getFieldProps('text')}
                  error={!!(touched['text'] && errors['text'])}
                  helperText={touched['text'] && errors['text']}
                />
              </Grid>
            </Grid>
          </Box>
          <Divider className={classes.divider} />
          <Box className={classes.controls}>
            <Button
              color="primary"
              variant="outlined"
              className={classes.cancelButton}
              data-testid="cancel-button"
              onClick={closeModal}
            >
              Cancel
            </Button>
            <LoadingButton
              type="submit"
              color="primary"
              variant="contained"
              text="Save"
              loading={saveDisabled}
              data-testid="save-button"
              className={classes.saveButton}
              onClick={submitNote}
            />
          </Box>
        </Box>
      </Fade>
    </Modal>
  );
}

export default AddNewNoteModal;
