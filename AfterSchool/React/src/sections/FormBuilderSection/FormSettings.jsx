import React from 'react';
import { Grid, Typography, Box, Button } from '@material-ui/core';
import useStyles from './styles';
import { useFormik, FormikProvider, Form } from 'formik';
import { useSnackbar } from 'notistack';

import * as Yup from 'yup';
import { useSearchParams, useParams } from 'react-router-dom';
import TextField from './components/TextField/TextField';
import TextAreaQuestion from './components/TextAreaQuestion';
import { useFormBuilder } from 'redux/hooks';

function FormSettings() {
  const classes = useStyles();
  const { formId } = useParams();
  const { enqueueSnackbar } = useSnackbar();

  const [searchParams, setSearchParams] = useSearchParams();

  const { form, questionsGroups, submitFormSectionData } = useFormBuilder();

  const validationSchema = Yup.object().shape({
    FormName: Yup.string().required('Form Name is required')
  });

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: form,
    validationSchema,
    onSubmit: async values => {
      await submitFormSectionData({ FormSettings: values }).then(() =>
        enqueueSnackbar('Submitted.', { variant: 'success' })
      );
    }
  });
  const { handleSubmit, values, getFieldProps, touched, errors } = formik;
  const firstSectionQuestionGroupId = questionsGroups.length
    ? questionsGroups[0].QuestionGroupDataTypeId
    : -1;

  return (
    <Grid item lg={9} xs={12} sm={6}>
      <Box>
        <Typography variant="h6" className={classes.sectionHeading}>
          Settings
        </Typography>
      </Box>

      <FormikProvider value={formik}>
        <Form autoComplete="off" noValidate className={classes.form}>
          <Box>
            <TextField
              formikProps={{
                ...getFieldProps('FormName'),
                error: touched['FormName'] && errors['FormName'],
                helperText: touched['FormName'] && errors['FormName']
              }}
              label={'Form Name'}
            />
            <TextField
              formikProps={{
                ...getFieldProps('FormType'),
                error: touched['FormType'] && errors['FormType'],
                helperText: touched['FormType'] && errors['FormType'],
                disabled: true
              }}
              label={'Form Type'}
            />
            <TextAreaQuestion
              formikProps={{
                ...getFieldProps('FormDescription'),
                error: touched['FormDescription'] && errors['FormDescription'],
                helperText: touched['FormDescription'] && errors['FormDescription'],
                errorMsg: errors['FormDescription']
              }}
              label={'Description'}
            />
          </Box>
          <Box className={classes.buttonOuterWrapper}>
            <Box className={classes.buttonWrapper}>
              <Button
                color="primary"
                variant="outlined"
                className={classes.backButton}
                type="submit"
                disabled={false}
                onClick={event => {
                  event.preventDefault();
                  handleSubmit();
                }}
                data-testid={`Back-${'Name'}`}
              >
                Save
              </Button>
              <Button
                data-testid={`Save-${'Name'}`}
                color="primary"
                className={classes.saveButton}
                variant="contained"
                // type="submit"
                disabled={false}
                key={'activeGroup'}
                onClick={() => {
                  setSearchParams(`qg=${firstSectionQuestionGroupId}`);
                }}
              >
                Get Started
              </Button>
            </Box>
          </Box>
        </Form>
      </FormikProvider>
    </Grid>
  );
}

export default FormSettings;
