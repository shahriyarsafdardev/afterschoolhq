import cs from 'classnames';
import { cloneDeep } from 'lodash';
import { useSnackbar } from 'notistack';
import { useFormik, FormikProvider, Form } from 'formik';
import { useSearchParams, useParams } from 'react-router-dom';
import React, { useEffect, useState, useCallback } from 'react';
import { Grid, CircularProgress, Typography, TextField, Box, Button } from '@material-ui/core';

import { useUnsavedChanges } from './UnsavedChangesProvider';
import { newQuestion, unSavedDialogProps } from './helper';
import formValidationSchema from './formValidationSchema';
import ApplyToAllControls from './ApplyToAllControls';
import { usePrompt } from 'utils/hooks/usePrompt';
import { useFormBuilder } from 'redux/hooks';
import QuestionsList from './QuestionsList';
import { Loader } from 'components';
import useStyles from './styles';
import Dialog from './Dialog';

function FormSectionSettings() {
  const classes = useStyles();
  const { formId } = useParams();
  const { enqueueSnackbar } = useSnackbar();
  const [searchParams, setSearchParams] = useSearchParams();
  const { unsavedChanges, markChangesAsUnsaved, markChangesAsSaved } = useUnsavedChanges();
  const [settingsForAll, setSettingsForAll] = useState({ show: [], required: [] });
  const [sectionNameError, setSectionNameError] = useState('');
  const [validationSchema, setValidationSchema] = useState({});
  const [saveDisabled, setSaveDisabled] = useState(false);
  const [initialValues, setInitialValues] = useState({});
  const [reRender, setReRender] = useState(false);
  const questionGroup = searchParams.get('qg');

  const {
    submitFormSectionData,
    loadSectionData,
    questionsGroups,
    clearQuestions,
    sectionLoading,
    questions,
    form,
    msgs
  } = useFormBuilder();

  const currentSectionIndex = questionsGroups.findIndex(qg => qg.Id == questionGroup);
  const currentSection = questionsGroups[currentSectionIndex];
  const [sectionName, setSectionName] = useState(currentSection?.Name || '');

  useEffect(() => {
    loadSectionData(formId, questionGroup);
    return () => {
      clearQuestions();
    };
  }, [formId, questionGroup]);

  useEffect(() => {
    setSectionName(currentSection?.Name || '');
  }, [currentSection?.Name]);

  useEffect(() => {
    const tempInitialValues = {};

    questions.forEach(question => {
      let tempQuestion = { ...question };
      tempQuestion.Options = tempQuestion.Options?.split(',').map(option => {
        const [key, value] = option.split('|');
        return { key, value };
      });
      tempInitialValues[tempQuestion.DisplayOrder] = tempQuestion;
    });

    setInitialValues(tempInitialValues);
  }, [questions]);

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: initialValues,
    validationSchema,
    onSubmit: async values => {
      setSaveDisabled(true);
      const formQuestions = Object.keys(values).map(key => {
        const updatedValue = { ...values[key] };

        const optionsString =
          (updatedValue.Options &&
            updatedValue.Options?.map(option => {
              if (option.value) {
                return `${option.key}|${option.value}`;
              } else {
                return option.key;
              }
            }).join(',')) ||
          '';

        updatedValue.Options = optionsString;
        return updatedValue;
      });

      const updatedQuestionGroups = [...questionsGroups];

      if (currentSection?.Name !== sectionName) {
        updatedQuestionGroups[currentSectionIndex] = { ...currentSection, Name: sectionName };
      }

      await submitFormSectionData({
        FormQuestionGroup: { ...currentSection, Name: sectionName },
        FormQuestionGroups: updatedQuestionGroups,
        FormSettings: { FormId: formId },
        FormQuestion: formQuestions
      }).then(() => {
        if (msgs.length) return;

        enqueueSnackbar('Submitted.', { variant: 'success' });
        setSaveDisabled(false);
      });
      markChangesAsSaved();
    }
  });

  const { handleSubmit, values, setFieldValue, setValues, dirty } = formik;
  const formDirty = dirty || currentSection?.Name !== sectionName;
  const [showPrompt, cancelNavigation, confirmNavigation] = usePrompt(formDirty);

  useEffect(() => {
    if (unsavedChanges) {
      window.onbeforeunload = () => true;
    } else {
      window.onbeforeunload = undefined;
    }

    return () => {
      window.onbeforeunload = undefined;
    };
  }, [unsavedChanges]);

  useEffect(() => {
    if (formDirty) {
      markChangesAsUnsaved();
    } else {
      markChangesAsSaved();
    }
  }, [formDirty]);

  useEffect(() => {
    setValidationSchema(formValidationSchema(values));
  }, [values]);

  const addNewQuestionHandler = index => {
    const questionsToShow = () =>
      Object.keys(values).reduce((qs, key) => {
        if (!values[key].IsDeleted) return [...qs, values[key]];

        return qs;
      }, []);
    const newDisplayOrder = Number(index || questionsToShow().at(-1)?.DisplayOrder || 0) + 1;

    const newQuest = {
      ...cloneDeep(newQuestion(form?.FormType)),
      ID: 0,
      GroupTitle: sectionName,
      FormQuestionGroupId: currentSection?.Id,
      DisplayOrder: newDisplayOrder
    };

    const updatedValues = { ...values };

    if (index !== undefined) {
      Object.keys(updatedValues)
        .reverse()
        .forEach(strKey => {
          const key = Number(strKey);
          if (key < newDisplayOrder) return;

          const questionObject = {
            ...updatedValues[key],
            DisplayOrder: key + 1
          };
          updatedValues[key + 1] = questionObject;
        });
    }

    updatedValues[newDisplayOrder] = newQuest;
    setValues({ ...updatedValues }, false);
    setReRender(!reRender);
  };

  const [timeoutConst, setTimeoutConst] = useState(null);

  const updateValue = value => {
    setSectionName(value);

    if (timeoutConst) {
      clearTimeout(timeoutConst);
    }
    const tempTimeout = setTimeout(() => {
      sectionNameChangeHandler(value);
    }, 1000);
    setTimeoutConst(tempTimeout);
  };

  const sectionNameChangeHandler = value => {
    const isNameUnique = questionsGroups.every(
      ({ Name: name, Id }) => name !== value || Id == questionGroup
    );

    if (value.length === 0) setSectionNameError('Section Name cannot be empty');
    else if (!isNameUnique) setSectionNameError('Section Name must be unique');
    else setSectionNameError('');
  };

  const updateAudienceSettings = useCallback(() => {
    if (!values) return;

    const audienceShowSettings = {
      show: [false, true, true, true],
      required: [false, true, true, true]
    };

    Object.values(values).forEach(value => {
      if (value.IsDeleted === false) {
        value.QuestionSettings.forEach(setting => {
          const audience = setting.Audience;
          if (!setting.Show) audienceShowSettings.show[audience] = false;
          if (!setting.Required) audienceShowSettings.required[audience] = false;
        });
      }
    });

    audienceShowSettings.show[0] = [1, 2, 3].every(audience => audienceShowSettings.show[audience]);
    audienceShowSettings.required[0] = [1, 2, 3].every(
      audience => audienceShowSettings.required[audience]
    );

    return audienceShowSettings;
  }, [values]);

  useEffect(() => {
    setSettingsForAll(updateAudienceSettings());
  }, [values]);

  const buttonText = text => {
    if (!saveDisabled) return text;

    return (
      <>
        {'Loading . . .'} <CircularProgress size={'20px'} />
      </>
    );
  };

  const formSaveHandler = event => {
    event.preventDefault();
    handleSubmit();
  };

  if (sectionLoading)
    return (
      <Grid item lg={9} md={9} xs={12} sm={12}>
        <Loader />
      </Grid>
    );

  return (
    <Grid item lg={9} md={9} xs={12} sm={12}>
      <FormikProvider value={formik}>
        <Form autoComplete="off" noValidate className={classes.form}>
          <Dialog
            open={showPrompt}
            onClose={confirmNavigation}
            onSubmit={cancelNavigation}
            {...unSavedDialogProps}
          />
          <Box>
            <Typography variant="h6" className={classes.sectionHeading}>
              Section Name
            </Typography>
            <TextField
              className={cs(classes.formControlClass, classes.sectionNameField)}
              size="small"
              fullWidth
              variant="outlined"
              value={sectionName}
              onChange={event => updateValue(event.target.value)}
              disabled={currentSection.QuestionGroupDataTypeId !== 0}
              helperText={sectionNameError}
              error={!!sectionNameError}
              data-testid={`sectionName-${questionGroup}`}
            />
          </Box>
          <Box>
            {form?.FormType !== 3 && (
              <ApplyToAllControls
                values={values}
                setValues={setValues}
                reRender={reRender}
                setReRender={setReRender}
                questionGroup={questionGroup}
                settingsForAll={settingsForAll}
              />
            )}
            <QuestionsList
              reRender={reRender}
              setValues={setValues}
              setReRender={setReRender}
              setFieldValue={setFieldValue}
              addNewQuestionHandler={addNewQuestionHandler}
              form={form}
              formik={formik}
            />
            <Box className={classes.buttonWrapper}>
              <Button
                color="primary"
                variant="outlined"
                className={classes.addNewQuestionButton}
                onClick={event => {
                  event.preventDefault();
                  addNewQuestionHandler();
                }}
                data-testid={`addNewQuestion-${questionGroup}`}
              >
                + Add New Question
              </Button>
              <Button
                type="submit"
                color="primary"
                variant="outlined"
                data-testid={`bottom-saveQuestions-${questionGroup}`}
                className={classes.saveButton}
                disabled={!unsavedChanges}
                onClick={formSaveHandler}
              >
                {buttonText('Save')}
              </Button>
              <Button
                type="submit"
                color="primary"
                variant="outlined"
                data-testid={`top-saveQuestions-${questionGroup}`}
                className={classes.saveFormButton}
                disabled={!unsavedChanges}
                onClick={formSaveHandler}
              >
                {buttonText('Save Form')}
              </Button>
            </Box>
          </Box>
        </Form>
      </FormikProvider>
    </Grid>
  );
}

export default FormSectionSettings;
