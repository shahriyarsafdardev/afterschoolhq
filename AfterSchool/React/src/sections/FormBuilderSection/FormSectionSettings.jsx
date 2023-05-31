import * as Yup from 'yup';
import cs from 'classnames';
import { cloneDeep } from 'lodash';
import { useSnackbar } from 'notistack';
import { useFormik, FormikProvider, Form } from 'formik';
import { useSearchParams, useParams } from 'react-router-dom';
import React, { useEffect, useState, useCallback, useRef } from 'react';
import { DragDropContext, Droppable, Draggable } from 'react-beautiful-dnd';
import { Grid, CircularProgress, Typography, TextField, Box, Button } from '@material-ui/core';

import Dialog from './Dialog';
import useStyles from './styles';
import { Loader } from 'components';
import { useFormBuilder } from 'redux/hooks';
import RenderQuestion from './RenderQuestion';
import RenderCheckbox from './RenderCheckbox';
import { audienceTypes, questionType } from 'constants';
import { useUnsavedChanges } from './UnsavedChangesProvider';
import { newQuestion, unSavedDialogProps, infoText } from './helper';

function FormSectionSettings() {
  const classes = useStyles();
  const { formId } = useParams();
  const { enqueueSnackbar } = useSnackbar();
  const [searchParams, setSearchParams] = useSearchParams();
  const { markChangesAsUnsaved, markChangesAsSaved } = useUnsavedChanges();
  const [settingsForAll, setSettingsForAll] = useState({ show: [], required: [] });
  const [sectionNameError, setSectionNameError] = useState('');
  const [validationSchema, setValidationSchema] = useState({});
  const [saveDisabled, setSaveDisabled] = useState(false);
  const [initialValues, setInitialValues] = useState({});
  const [openModal, setOpenModal] = useState(false);
  const [reRender, setReRender] = useState(false);
  const questionGroup = searchParams.get('qg');
  const backUrl = useRef();

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

  const currentSectionIndex = questionsGroups.findIndex(qg => qg.ID == questionGroup);
  const currentSection = questionsGroups[currentSectionIndex];
  const [sectionName, setSectionName] = useState(currentSection?.Name || '');

  const { dropdown, radioList, checkBoxList, textBlock } = questionType;
  const isQuestionWithOptions = questionTypeId =>
    [dropdown, radioList, checkBoxList].includes(questionTypeId);

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
  }, [questions?.length]);

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
        FormSettings: { FormId: formId },
        FormQuestionGroup: { ...currentSection, Name: sectionName },
        FormQuestion: formQuestions,
        FormQuestionGroups: updatedQuestionGroups
      }).then(() => {
        if (msgs.length) return;

        enqueueSnackbar('Submitted.', { variant: 'success' });
        setSaveDisabled(false);
      });
      markChangesAsSaved();
    }
  });

  const { handleSubmit, values, setFieldValue, setValues, errors, dirty, touched } = formik;
  const formDirty = dirty || currentSection?.Name !== sectionName;

  useEffect(() => {
    const backButtonClicked = e => {
      e.preventDefault();
      setOpenModal(true);
    };

    if (formDirty) {
      if (backUrl.current !== window.location.href) {
        backUrl.current = window.location.href;
        window.history.pushState(null, null, window.location.href);
      }

      window.addEventListener('popstate', backButtonClicked);
    } else {
      setOpenModal(false);
      window.removeEventListener('popstate', backButtonClicked);
    }

    return () => window.removeEventListener('popstate', backButtonClicked);
  }, [window.location.href, formDirty]);

  useEffect(() => {
    if (formDirty) {
      window.onbeforeunload = () => true;
    } else {
      window.onbeforeunload = undefined;
    }

    return () => {
      window.onbeforeunload = undefined;
    };
  }, [formDirty]);

  useEffect(() => {
    if (formDirty) {
      markChangesAsUnsaved();
    } else {
      markChangesAsSaved();
    }
  }, [formDirty]);

  useEffect(() => {
    setValidationSchema(
      Yup.object().shape(
        Object.keys(values).reduce((acc, key) => {
          acc[key] = Yup.object().shape({
            QuestionTypeId: Yup.number(),
            DisplayOrder: Yup.number(),
            Text: Yup.string().when(['QuestionTypeId'], {
              is: val => val !== textBlock,
              then: () =>
                Yup.string()
                  .required('Text is Required')
                  .max(400, 'Text cannot be longer than 400 characters'),
              otherwise: () =>
                Yup.string()
                  .required('Text is Required')
                  .max(4000, 'Text cannot be longer than 4000 characters')
            }),
            Options: Yup.array().when(['QuestionTypeId'], {
              is: val => isQuestionWithOptions(val),
              then: () =>
                Yup.array()
                  .required('Options are required')
                  .of(
                    Yup.object().shape({
                      key: Yup.string().required('Text is required'),
                      value: Yup.string()
                    })
                  )
                  .test(
                    'options-length',
                    'Atleast 2 options are required',
                    options => options.length >= 2
                  ),
              otherwise: () => Yup.array().notRequired()
            })
          });
          return acc;
        }, {})
      )
    );
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
      FormQuestionGroupId: currentSection?.ID,
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
  };

  const handleDelete = (key, removeFromList = false) => {
    const { [key]: deletedValue, ...restValues } = values;
    const newValues = {};
    Object.keys(restValues).forEach(k => {
      const newRow = Number(k) > Number(key) ? Number(k) - 1 : k;
      newValues[newRow] = { ...restValues[k], DisplayOrder: newRow };
    });

    if (!removeFromList) {
      const newKey = Object.keys(newValues).length + 1;
      newValues[newKey] = { ...deletedValue, IsDeleted: true, DisplayOrder: newKey };
    }

    setValues({ ...newValues }, false);
    setReRender(!reRender);
  };

  const handleCopy = key => {
    const copiedValue = cloneDeep(values[key]);

    const newValues = Object.keys(values).reduce((acc, k) => {
      const newKey = Number(k) > Number(key) ? Number(k) + 1 : k;
      acc[newKey] = cloneDeep(values[k]);
      acc[newKey].DisplayOrder = Number(newKey);
      return acc;
    }, {});

    newValues[Number(key) + 1] = {
      ...copiedValue,
      ID: 0,
      QuestionDataTypeId: 0,
      DisplayOrder: Number(key) + 1,
      IsLocked: false
    };

    setValues({ ...newValues }, false);
    setReRender(!reRender);
  };

  const moveObjectInFormikState = (source, destination) => {
    const sourceValue = cloneDeep(values[source]);
    const newState = cloneDeep(values);

    if (source < destination) {
      for (let i = source; i < destination; i++) {
        newState[i] = { ...newState[i + 1], DisplayOrder: Number(i) };
      }
    } else {
      for (let i = source; i > destination; i--) {
        newState[i] = { ...newState[i - 1], DisplayOrder: Number(i) };
      }
    }

    newState[destination] = { ...sourceValue, DisplayOrder: Number(destination) };

    setValues(newState, false);
    setReRender(!reRender);
  };

  const onDragEnd = result => {
    const { source, destination } = result;
    if (!destination) return;

    const canNotDrop = !!values[destination.index].IsLocked;
    if (canNotDrop) return;

    moveObjectInFormikState(source.index, destination.index);
  };

  const handleSettingForAll = (propertyName, registrantType, val) => {
    const updatedQuestionSettings = settings => {
      let updatedSettings = {};
      const audienceType = audienceTypes.indexOf(registrantType);

      if (audienceType === 0) {
        updatedSettings = settings.map(setting => {
          let updatedSetting = { ...setting };
          switch (propertyName) {
            case 'show':
              updatedSetting.Show = val;
              updatedSetting.Required = val ? setting.Required : false;
              break;
            case 'required':
              updatedSetting.Required = val;
              updatedSetting.Show = val ? true : setting.Show;
              break;
            default:
              break;
          }

          return updatedSetting;
        });
      } else {
        updatedSettings = settings.map(setting => {
          if (setting.Audience !== audienceTypes.indexOf(registrantType)) {
            return setting;
          }

          let updatedSetting = { ...setting };
          switch (propertyName) {
            case 'show':
              updatedSetting.Show = val;
              updatedSetting.Required = val ? setting.Required : false;
              break;
            case 'required':
              updatedSetting.Required = val;
              updatedSetting.Show = val ? true : setting.Show;
              break;
            default:
              break;
          }

          return updatedSetting;
        });
      }
      return updatedSettings;
    };
    const newValues = { ...values };

    Object.keys(newValues).forEach(key => {
      if (newValues[key].IsLocked) {
        return;
      }

      newValues[key] = {
        ...newValues[key],
        QuestionSettings: updatedQuestionSettings(newValues[key].QuestionSettings)
      };
    });

    setReRender(!reRender);
    setValues(newValues, false);
  };
  const ApplyToAllControls = () => {
    return (
      <Box>
        <Typography variant="h6" className={classes.applyAllHeading}>
          Apply to all in this section:
        </Typography>
        <Typography variant="body2" className={classes.infoText}>
          {infoText}
        </Typography>
        <Grid container className={classes.controlsContainer}>
          <Grid container item lg={12} sm={12} xs={12} className={classes.formControlsRow}>
            <Grid item lg={3} md={3} sm={3} xs={3}>
              <Typography variant="body1" className={classes.applyAllRow}>
                Show For:
              </Typography>
            </Grid>
            <Grid
              container
              item
              lg={9}
              sm={9}
              xs={9}
              className={classes.applyToAllControlsContainer}
            >
              {audienceTypes.map((key, i) => (
                <Grid
                  item
                  lg={3}
                  md={3}
                  sm={6}
                  xs={6}
                  key={key}
                  className={classes.applyToAllControlsItem}
                >
                  <RenderCheckbox
                    key={key}
                    checked={settingsForAll?.show[i]}
                    handleCheck={event => {
                      handleSettingForAll('show', key, event.target.checked);
                    }}
                    dataTestId={`ApplyAll-${questionGroup}-${key}-show`}
                  />
                  <Typography variant="body1" className={classes.applyToAllControlsWrapper}>
                    {key}
                  </Typography>
                </Grid>
              ))}
            </Grid>
          </Grid>
          <Grid container item lg={12} sm={12} xs={12} className={classes.formControlsRow}>
            <Grid item lg={3} sm={3} xs={3}>
              <Typography variant="body1" className={classes.applyAllRow}>
                Required For:
              </Typography>
            </Grid>
            <Grid container className={classes.applyToAllControlsContainer}>
              {audienceTypes.map((key, i) => (
                <Grid
                  item
                  lg={3}
                  md={3}
                  sm={6}
                  xs={6}
                  key={key}
                  className={classes.applyToAllControlsItem}
                >
                  <RenderCheckbox
                    key={key}
                    checked={settingsForAll?.required[i]}
                    handleCheck={event => {
                      handleSettingForAll('required', key, event.target.checked);
                    }}
                    dataTestId={`ApplyAll-${questionGroup}-${key}-required`}
                  />
                  <Typography variant="body1" className={classes.applyToAllControlsWrapper}>
                    {key}
                  </Typography>
                </Grid>
              ))}
            </Grid>
          </Grid>
        </Grid>
      </Box>
    );
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
      ({ Name: name, ID }) => name !== value || ID == questionGroup
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

  const disableSaveButton = !formDirty || sectionNameError || saveDisabled;
  const formSaveHandler = event => {
    event.preventDefault();
    handleSubmit();
  };

  if (sectionLoading) return <Loader />;

  return (
    <Grid item lg={9} md={9} xs={12} sm={12}>
      <FormikProvider value={formik}>
        <Form autoComplete="off" noValidate className={classes.form}>
          <Dialog
            open={openModal}
            onClose={() => {
              window.history.pushState(null, null, window.location.href);
              setOpenModal(false);
            }}
            onSubmit={() => {
              setOpenModal(false);
              window.history.back();
            }}
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
              error={sectionNameError}
              data-testid={`sectionName-${questionGroup}`}
            />
          </Box>
          <Box>
            {form?.FormType !== 3 && <ApplyToAllControls />}
            <Box>
              {values && (
                <DragDropContext onDragEnd={onDragEnd}>
                  <Droppable key="list-2" droppableId="dropable-questions-container" type="column">
                    {provided => (
                      <div ref={provided.innerRef} {...provided.droppableProps}>
                        {Object.keys(values).map(
                          key =>
                            !values[key].IsDeleted && (
                              <Draggable
                                isDragDisabled={values[key].IsLocked}
                                draggableId={`question-${key}`}
                                index={Number(key)}
                                key={key}
                              >
                                {(provided, snapshot) => (
                                  <Box
                                    ref={provided.innerRef}
                                    {...provided.draggableProps}
                                    className={
                                      snapshot.isDragging
                                        ? classes.whiteBorder
                                        : classes.transparentBorder
                                    }
                                  >
                                    <RenderQuestion
                                      index={key}
                                      key={`${key}-${values[key]?.ID}${reRender}`}
                                      value={values[key]}
                                      setFieldValue={setFieldValue}
                                      handleDelete={handleDelete}
                                      handleCopy={handleCopy}
                                      handleAddNewQuestion={addNewQuestionHandler}
                                      dragIndicatorProps={provided.dragHandleProps}
                                      errors={errors[key]}
                                      touched={touched[key]}
                                      formType={form?.FormType}
                                    />
                                  </Box>
                                )}
                              </Draggable>
                            )
                        )}
                        {provided.placeholder}
                      </div>
                    )}
                  </Droppable>
                </DragDropContext>
              )}
            </Box>
            <Box className={classes.buttonWrapper}>
              <Button
                color="primary"
                variant="outlined"
                style={{
                  color: '#494949',
                  fontSize: '14px',
                  fontWeight: 'bold',
                  border: '1px solid #E4E4E4'
                }}
                disabled={false}
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
                disabled={disableSaveButton}
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
                disabled={disableSaveButton}
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
