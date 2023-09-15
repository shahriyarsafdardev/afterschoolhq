import * as Yup from 'yup';
import cs from 'classnames';
import { isEmpty } from 'lodash';
import { useSnackbar } from 'notistack';
import React, { useEffect } from 'react';
import LinkIcon from '@material-ui/icons/Link';
import { useFormik, FormikProvider, Form } from 'formik';
import { useParams, useSearchParams } from 'react-router-dom';
import { Grid, Typography, Box, Button, ButtonGroup } from '@material-ui/core';

import TextField from './components/TextField/TextField';
import TextAreaQuestion from './components/TextAreaQuestion';
import { useUnsavedChanges } from './UnsavedChangesProvider';
import { useFormBuilder } from 'redux/hooks';
import { CopyClipBoard } from 'components';
import { formTypes } from 'constants';
import useStyles from './styles';

function FormSettings() {
  const classes = useStyles();
  const { formId } = useParams();
  const { enqueueSnackbar } = useSnackbar();
  const [, setSearchParams] = useSearchParams();
  const { form, questionsGroups, updateFormSettings, publishOrUnpublishSurvey } = useFormBuilder();
  const { markChangesAsUnsaved, markChangesAsSaved } = useUnsavedChanges();
  const firstSectionQuestionGroupId = questionsGroups.length ? questionsGroups[0].Id : -1;
  const { FormType: formType, IsPublished: isPublished, PublishedURL: publishedURL } = form;

  const validationSchema = Yup.object().shape({
    FormName: Yup.string().required('Form Name is required'),
    FormDescription: Yup.string().max(400, 'Text cannot be longer than 400 characters')
  });

  const updatedForm = { ...form, FormDescription: form.FormDescription || '' };

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: updatedForm,
    validationSchema,
    onSubmit: async values => {
      await updateFormSettings({
        FormSettings: values
      }).then(() => enqueueSnackbar('Submitted.', { variant: 'success' }));
    }
  });

  const { handleSubmit, values, getFieldProps, touched, errors, dirty } = formik;

  useEffect(() => {
    if (dirty) {
      markChangesAsUnsaved();
    } else {
      markChangesAsSaved();
    }
  }, [dirty]);

  const handleSurveyPublish = formId => {
    publishOrUnpublishSurvey(formId).then(res => {
      enqueueSnackbar(res ? 'Published' : 'Unpublished', { variant: 'success' });
    });
  };

  return (
    <Grid item lg={9} md={9} xs={12} sm={12}>
      <Box>
        <Typography variant="h6" className={classes.sectionHeading}>
          Settings
        </Typography>
        {formType === 2 && (
          <ButtonGroup
            disableElevation
            variant="outlined"
            color="primary"
            className={classes.publishButtonsGroup}
          >
            <Button
              type="submit"
              data-testid={`publish-unPublish-${formId}`}
              onClick={() => handleSurveyPublish(formId)}
              className={cs(classes.publishButton, isPublished ? classes.publishBtnscolors : '')}
            >
              {isPublished ? 'Unpublish' : 'Publish'}
            </Button>
            {form.IsPublished && (
              <Button>
                <CopyClipBoard textToCopy={publishedURL}>
                  {<LinkIcon fontSize="large" />}
                </CopyClipBoard>
              </Button>
            )}
          </ButtonGroup>
        )}
      </Box>

      <FormikProvider value={formik}>
        <Form autoComplete="off" noValidate className={classes.form}>
          <Box className={classes.marginTop}>
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
                error: values['FormDescription']?.length > 400,
                helperText: touched['FormDescription'] && errors['FormDescription'],
                errorMsg: errors['FormDescription']
              }}
              label="Internal Description"
              data-testid="Description"
            />
          </Box>
          <Box className={classes.buttonOuterWrapper}>
            <Box className={classes.buttonWrapper}>
              <Button
                color="primary"
                variant="outlined"
                className={classes.backButton}
                type="submit"
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
                onClick={() => {
                  dirty && handleSubmit();
                  isEmpty(errors) && setSearchParams(`qg=${firstSectionQuestionGroupId}`);
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
