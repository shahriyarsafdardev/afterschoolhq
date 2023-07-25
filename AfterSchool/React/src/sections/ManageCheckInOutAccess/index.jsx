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
    data: { Students = [] },
    updateAuthorizedPersons,
    loadAuthorizedPersonsList
  } = useManageAPAccess();

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
    onSubmit: async values => {
      const students = [];
      Object.keys(values).map(key => {
        const student = {
          AuthorizedPersons: values[key],
          StudentName: Students[key].StudentName,
          StudentUid: Students[key].StudentUid
        };
        students.push(student);
      });
      updateAuthorizedPersons(students).then(() => {
        enqueueSnackbar('Submitted.', { variant: 'success' });
        resetForm();
      });
    }
  });

  const { setFieldValue, values, handleSubmit, resetForm, dirty } = formik;

  const handleOnCancel = () => {
    resetForm();
  };

  const handleSubmitForm = () => {
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
          type="submit"
          onClick={handleSubmitForm}
          variant="contained"
          color="primary"
          className={classes.saveButton}
          text={'Save'}
          disabled={!dirty}
          loading={false}
        />
      </Box>
    </Box>
  );
}
