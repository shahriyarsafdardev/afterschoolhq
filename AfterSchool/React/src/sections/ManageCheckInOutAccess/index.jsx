import { isEmpty } from 'lodash';
import { useFormik } from 'formik';
import { useSnackbar } from 'notistack';
import React, { useEffect, useState } from 'react';
import { Box, Button } from '@material-ui/core';

import useStyles from './styles';
import { Loader } from 'components';
import { useManageAPAccess } from 'redux/hooks';
import LoadingButton from 'components/Overrides/LoadingButton';
import PinCodeSection from './PinCodeSection';
import AuthPersonsManagementSection from './AuthPersonsManagementSection';

export default function index() {
  const classes = useStyles();
  const { enqueueSnackbar } = useSnackbar();
  const [initialValues, setInitialValue] = useState({});

  const {
    msgs,
    data,
    loading,
    updateAuthorizedPersons,
    loadAuthorizedPersonsList
  } = useManageAPAccess();

  const { Students = [], IsPinSetup, IsPhoneSetup, RegistrantType } = data || {};
  const hasAccessToManageAPA = RegistrantType === 'Parent';

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
      <Box className={classes.sectionWrapper}>
        <PinCodeSection IsPinSetup={IsPinSetup} IsPhoneSetup={IsPhoneSetup} />
      </Box>
      {hasAccessToManageAPA && (
        <Box className={classes.sectionWrapper}>
          <AuthPersonsManagementSection
            values={values}
            setFieldValue={setFieldValue}
            Students={Students}
          />
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
      )}
    </Box>
  );
}
