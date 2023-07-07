import React, { useEffect } from 'react';
import { useSearchParams, useParams } from 'react-router-dom';
import { useSnackbar } from 'notistack';
import { Grid } from '@material-ui/core';

import useStyles from './styles';
import { Loader } from 'components';
import FormSettings from './FormSettings';
import { useFormBuilder } from 'redux/hooks';
import FormSectionList from './FormSectionList';
import FormSectionSettings from './FormSectionSettings';
import { UnsavedChangesProvider } from './UnsavedChangesProvider';

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

  if (loading) return <Loader />;

  return (
    <UnsavedChangesProvider>
      <Grid container className={classes.wrapper}>
        {questionGroup === null ? <FormSettings /> : <FormSectionSettings />}
        <FormSectionList />
      </Grid>
    </UnsavedChangesProvider>
  );
};
export default FormBuilderSection;
