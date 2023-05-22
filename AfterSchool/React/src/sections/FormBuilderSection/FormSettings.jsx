import React from 'react';
import * as Yup from 'yup';
import { useSnackbar } from 'notistack';
import { useSearchParams } from 'react-router-dom';
import { useFormik, FormikProvider, Form } from 'formik';
import { Grid, Typography, Box, Button } from '@material-ui/core';

import TextField from './components/TextField/TextField';
import TextAreaQuestion from './components/TextAreaQuestion';
import { useFormBuilder } from 'redux/hooks';
import { formTypes } from 'constants';
import useStyles from './styles';

function FormSettings() {
  const classes = useStyles();
  const { enqueueSnackbar } = useSnackbar();
  const [searchParams, setSearchParams] = useSearchParams();
  const { form, questionsGroups, updateFormSettings } = useFormBuilder();
  const firstSectionQuestionGroupId = questionsGroups.length ? questionsGroups[0].ID : -1;

  const validationSchema = Yup.object().shape({
    FormName: Yup.string().required('Form Name is required')
  });

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: form,
    validationSchema,
    onSubmit: async values => {
      await updateFormSettings({
        FormSettings: values
      }).then(() => enqueueSnackbar('Submitted.', { variant: 'success' }));
    }
  });

  const { handleSubmit, values, getFieldProps, touched, errors, dirty } = formik;

  return (
    <Grid item lg={9} md={9} xs={12} sm={12}>
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
              data-testid={'Form Name'}
            />
            <TextField
              formikProps={{
                value: formTypes[values['FormType']],
                error: touched['FormType'] && errors['FormType'],
                helperText: touched['FormType'] && errors['FormType'],
                disabled: true
              }}
              label={'Form Type'}
              data-testid={'Form Type'}
            />
            <TextAreaQuestion
              formikProps={{
                ...getFieldProps('FormDescription'),
                error: touched['FormDescription'] && errors['FormDescription'],
                helperText: touched['FormDescription'] && errors['FormDescription'],
                errorMsg: errors['FormDescription']
              }}
              label={'Description'}
              data-testid={'Description'}
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
                data-testid={'SaveFormSettingsButton'}
              >
                Save
              </Button>
              <Button
                data-testid={'GetStartedButton'}
                color="primary"
                className={classes.getStartedButton}
                variant="contained"
                disabled={false}
                key={'activeGroup'}
                onClick={() => {
                  dirty && handleSubmit();
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
