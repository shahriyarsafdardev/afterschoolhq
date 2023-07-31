import React, { useEffect, useState } from 'react';
import { Loader } from 'components';
import { useFormik } from 'formik';
import { useSnackbar } from 'notistack';
import { Box, Button, Typography } from '@material-ui/core';
import LoadingButton from 'components/Overrides/LoadingButton';

import AuthorizedPersonListSetup from './AuthorizedPersonListSetup';
import { useManageAPAccess } from 'redux/hooks';
import useStyles from './styles';
import { isEmpty } from 'lodash';

export default function index() {
  const classes = useStyles();
  const { enqueueSnackbar } = useSnackbar();
  const [initialValues, setInitialValue] = useState({});

  const {
    msgs,
    loading,
    data,
    updateAuthorizedPersons,
    loadAuthorizedPersonsList
  } = useManageAPAccess();

  const { Students = [] } = data || {};

  useEffect(() => {
    loadAuthorizedPersonsList();
  }, []);

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  useEffect(() => {
    if (isEmpty(Students)) return;
    const initialState = {};
    Students.forEach((student, index) => (initialState[index] = student.AuthorizedPersons));
    setInitialValue(initialState);
  }, [Students]);

  const formik = useFormik({
    enableReinitialize: true,
    initialValues,
    onSubmit: async (values, { setSubmitting, resetForm }) => {
      setSubmitting(true);
      const students = [];
      Object.keys(values).map(key => {
        const student = {
          AuthorizedPersons: values[key],
          StudentName: Students[key].StudentName,
          StudentUid: Students[key].StudentUid
        };
        students.push(student);
      });
      await updateAuthorizedPersons(students).then(msgs => {
        if (!msgs.length) {
          enqueueSnackbar('Submitted.', { variant: 'success' });
          resetForm();
        }
      });
      setSubmitting(false);
    }
  });

  const { setFieldValue, values, handleSubmit, resetForm, dirty, isSubmitting } = formik;

  const handleOnCancel = event => {
    event.preventDefault();
    resetForm();
  };

  const handleSubmitForm = event => {
    event.preventDefault();
    handleSubmit();
  };

  if (loading) return <Loader />;

  return (
    <Box>
      <Typography className={classes.heading}>
        New authorized person(s) will be notified and prompted to create their own PIN code for
        check in and check out.
      </Typography>
      <Typography className={classes.italicFont}>
        These settings will apply to all activities.
      </Typography>
      <Box className={classes.authListOuterWrapper}>
        {Object.keys(values).map(key => (
          <AuthorizedPersonListSetup
            key={key}
            studentName={Students[key]?.StudentName}
            studentUid={Students[key]?.StudentUid}
            authorizedPersons={values[key]}
            setValue={value => setFieldValue(key, value)}
          />
        ))}
      </Box>
      <Box className={classes.buttons}>
        <Button
          color="primary"
          variant="outlined"
          className={classes.cancelButton}
          data-testid="cancel-button"
          onClick={handleOnCancel}
        >
          Cancel
        </Button>
        <LoadingButton
          key="CICO-Save-Button"
          id="CICO-Save-Button"
          type="submit"
          onClick={handleSubmitForm}
          variant="contained"
          color="primary"
          className={classes.saveButton}
          text={'Save'}
          disabled={!dirty}
          loading={isSubmitting}
        />
      </Box>
    </Box>
  );
}
