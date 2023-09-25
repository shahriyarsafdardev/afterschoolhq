import React, { useEffect } from 'react';
import { useSearchParams, useParams } from 'react-router-dom';
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
  const [searchParams, setSearchParams] = useSearchParams();
  const questionGroup = searchParams.get('qg');
  const { questionsGroups, loading, loadFormData } = useFormBuilder();

  useEffect(() => {
    if (!questionsGroups.length) loadFormData(formId);
  }, [questionGroup]);

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
