import { useFormik } from 'formik';
import React, { useEffect } from 'react';
import { useSearchParams, useParams } from 'react-router-dom';
import { useSnackbar } from 'notistack';
import { Grid } from '@material-ui/core';

import { useFormBuilder } from 'redux/hooks';
import { Loader } from 'components';

import useStyles from './styles';
import FormSettings from './FormSettings';
import FormSectionList from './FormSectionList';
import FormSectionSettings from './FormSectionSettings';

const FormBuilderSection = () => {
  const classes = useStyles();
  const { formId } = useParams();
  const { enqueueSnackbar } = useSnackbar();
  const [searchParams, setSearchParams] = useSearchParams();
  const questionGroup = searchParams.get('qg');

  const { msgs, questionsGroups, loading, loadFormData } = useFormBuilder();

  useEffect(() => {
    if (!questionsGroups.length) loadFormData(formId);
  }, [questionGroup]);

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  return (
    <>
      {loading ? (
        <Loader />
      ) : (
        <Grid container className={classes.wrapper}>
          {questionGroup === null ? <FormSettings /> : <FormSectionSettings />}
          <FormSectionList />
        </Grid>
      )}
    </>
  );
};
export default FormBuilderSection;
