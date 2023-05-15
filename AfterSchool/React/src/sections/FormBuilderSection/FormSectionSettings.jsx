import * as Yup from 'yup';
import React, { useEffect, useState } from 'react';
import { useFormik, FormikProvider, Form } from 'formik';
import { useSearchParams, useParams } from 'react-router-dom';
import { Grid, Typography, TextField, Box, Button } from '@material-ui/core';
import { DragDropContext, Droppable, Draggable } from 'react-beautiful-dnd';

import useStyles from './styles';
import { toInteger } from 'lodash';
import { Loader } from 'components';
import { newQuestion } from './helper';
import { useFormBuilder } from 'redux/hooks';
import RenderQuestion from './RenderQuestion';
import RenderCheckbox from './RenderCheckbox';
import questionType from 'constants/questionType';

function FormSectionSettings() {
  const classes = useStyles();
  const { formId } = useParams();
  const initialState = {
    All: false,
    Parent: false,
    Student: false,
    Adult: false
  };
  const [initialValues, setInitialValues] = useState({});
  const [validationSchema, setValidationSchema] = useState({});
  const [searchParams, setSearchParams] = useSearchParams();
  const questionGroup = searchParams.get('qg');

  const {
    submitFormSectionData,
    loadSectionData,
    questionsGroups,
    sectionLoading,
    questions
  } = useFormBuilder();

  const currentSectionIndex = questionsGroups.findIndex(qg => qg.ID == questionGroup);
  const currentSection = questionsGroups[currentSectionIndex];
  const [sectionName, setSectionName] = useState(currentSection?.Name || '');

  useEffect(() => {
    loadSectionData(formId, questionGroup);
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

  const { dropdown, radioList, checkBoxList } = questionType;
  const isQuestionWithOptions = questionTypeId =>
    [dropdown, radioList, checkBoxList].includes(questionTypeId);

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: initialValues,
    validationSchema,
    onSubmit: async values => {
      const formQuestions = Object.keys(values).map(key => {
        const updatedValue = { ...values[key] };
        const shouldHaveOptions = isQuestionWithOptions(updatedValue.QuestionTypeId);

        if (shouldHaveOptions) {
          const optionsString =
            updatedValue.Options?.map(option => {
              if (option.value) {
                return `${option.key}|${option.value}`;
              } else {
                return option.key;
              }
            }).join(',') || '';
          updatedValue.Options = optionsString;
        }
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
      });
      console.log(values, formQuestions, 'submitted successfully');
    }
  });

  const { handleSubmit, values, setFieldValue, setValues, errors, touched } = formik;

  useEffect(() => {
    setValidationSchema(
      Yup.object().shape(
        Object.keys(values).reduce((acc, key) => {
          acc[key] = Yup.object().shape({
            QuestionTypeId: Yup.number(),
            DisplayOrder: Yup.number(),
            Text: Yup.string()
              .required('Text is required')
              .max(400, 'Text cannot be longer than 400 characters'),
            Options: Yup.array().when(['QuestionTypeId'], {
              is: val => isQuestionWithOptions(val),
              then: () =>
                Yup.array()
                  .required('Options are required')
                  .of(
                    Yup.object().shape({
                      key: Yup.string().required('Option key is required'),
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
  }, [values && Object.keys(values).length]);

  const addNewQuestionHandler = index => {
    const newDisplayOrder =
      toInteger(index || values[Object.keys(values).at(-1)]?.DisplayOrder) + 1;

    const newQuest = {
      ...newQuestion,
      GroupTitle: sectionName,
      FormQuestionGroupId: currentSection?.ID,
      DisplayOrder: newDisplayOrder
    };

    const updatedValues = { ...values };

    if (index !== undefined) {
      Object.keys(updatedValues)
        .reverse()
        .forEach(strKey => {
          const key = toInteger(strKey);
          if (key < newDisplayOrder) return;

          const questionObject = {
            ...updatedValues[key],
            DisplayOrder: key + 1
          };
          updatedValues[key + 1] = questionObject;
        });
    }

    updatedValues[newDisplayOrder] = newQuest;
    setValues({ ...updatedValues });
  };

  const handleDelete = key => {
    const { [key]: deletedValue, ...restValues } = values;
    const newValues = {};
    Object.keys(restValues).forEach(k => {
      const newRow = Number(k) > Number(key) ? Number(k) - 1 : k;
      newValues[newRow] = { ...restValues[k], DisplayOrder: newRow };
    });

    const newKey = Object.keys(newValues).length + 1;
    newValues[newKey] = { ...deletedValue, IsDeleted: true, DisplayOrder: newKey };

    setValues({ ...newValues });
  };

  const handleCopy = key => {
    const { [key]: copiedValue, ...restValues } = values;

    const newValues = Object.keys(values).reduce((acc, k) => {
      const newKey = Number(k) > Number(key) ? Number(k) + 1 : k;
      acc[newKey] = { ...values[k], DisplayOrder: newKey };
      return acc;
    }, {});

    newValues[Number(key) + 1] = {
      ...copiedValue,
      ID: 0,
      QuestionDataTypeId: 0,
      DisplayOrder: Number(key) + 1
    };
    formik.setValues(newValues);
  };

  const handleCreateNew = key => {
    const copiedValue = values[key];

    const newValues = Object.keys(values).reduce((acc, k) => {
      const newKey = Number(k) > Number(key) ? Number(k) + 1 : k;
      acc[newKey] = values[k];
      return acc;
    }, {});

    newValues[Number(key) + 1] = { ...copiedValue, ID: 0 };
    formik.setValues(newValues);
  };

  const moveObjectInFormikState = (source, destination) => {
    const sourceValue = values[source];
    const newState = { ...values };

    if (source < destination) {
      for (let i = source; i < destination; i++) {
        newState[i] = { ...newState[i + 1], DisplayOrder: Number(i) };
      }
    } else {
      for (let i = source; i > destination; i--) {
        newState[i] = { ...newState[i - 1], DisplayOrder: Number(i) };
      }
    }

    newState[destination] = sourceValue;

    setValues(newState);
  };

  const onDragEnd = result => {
    const { source, destination } = result;
    if (!destination) return;

    moveObjectInFormikState(source.index, destination.index);
  };

  const [reRender, setReRender] = useState(false);
  const [showFor, setShowFor] = useState(initialState);
  const [requiredFor, setRequiredFor] = useState(initialState);

  const handleSettingForAll = (propertyName, registrantType) => {
    const audienceTypes = ['All', 'Parent', 'Student', 'Adult'];
    const updatedQuestionSettings = settings => {
      const updatedSettings = settings.map(setting => {
        if (setting.Audience !== audienceTypes.indexOf(registrantType)) {
          return setting;
        }

        let updatedSetting = { ...setting };
        switch (propertyName) {
          case 'show':
            updatedSetting.Show = !showFor[registrantType];
            updatedSetting.Required = showFor[registrantType] ? false : setting.Required;
            break;
          case 'required':
            updatedSetting.Required = !requiredFor[registrantType];
            updatedSetting.Show = !requiredFor[registrantType] ? true : setting.Show;
            break;
          default:
            break;
        }

        return updatedSetting;
      });
      return updatedSettings;
    };
    const newValues = { ...values };

    Object.keys(newValues).forEach(key => {
      newValues[key] = {
        ...newValues[key],
        QuestionSettings: updatedQuestionSettings(newValues[key].QuestionSettings)
      };
    });

    formik.resetForm();
    setReRender(!reRender);
    setValues(newValues);
  };
  const ApplyToAllControls = () => {
    return (
      <Box>
        <Typography variant="h6" className={classes.applyAllHeading}>
          Apply to all in this section:
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
              {Object.keys(showFor).map(key => (
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
                    checked={showFor[key]}
                    handleCheck={() => {
                      handleSettingForAll('show', key);
                      setShowFor(prevState => ({ ...prevState, [key]: !showFor[key] }));
                    }}
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
              {Object.keys(requiredFor).map(key => (
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
                    checked={requiredFor[key]}
                    handleCheck={() => {
                      handleSettingForAll('required', key);
                      setRequiredFor(prevState => ({ ...prevState, [key]: !requiredFor[key] }));
                    }}
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

  return (
    <Grid item lg={9} md={9} xs={12} sm={12}>
      {sectionLoading ? (
        <Loader />
      ) : (
        <FormikProvider value={formik}>
          <Form autoComplete="off" noValidate className={classes.form}>
            <Box>
              <Typography variant="h6" className={classes.sectionHeading}>
                Section Name
              </Typography>
              <TextField
                className={classes.formControlClass}
                size="small"
                fullWidth
                variant="outlined"
                value={sectionName}
                onChange={event => setSectionName(event.target.value)}
                disabled={currentSection.QuestionGroupDataTypeId !== 0}
              />
            </Box>
            <Box>
              <ApplyToAllControls />
              <Box>
                {values && (
                  <DragDropContext onDragEnd={onDragEnd}>
                    <Droppable
                      key="list-2"
                      droppableId="dropable-questions-container"
                      type="column"
                    >
                      {provided => (
                        <div ref={provided.innerRef} {...provided.droppableProps}>
                          {Object.keys(values).map(
                            key =>
                              !values[key].IsDeleted && (
                                <Draggable
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
                  data-testid={`Back-${'Name'}`}
                >
                  + Add New Question
                </Button>
                <Button
                  color="primary"
                  variant="outlined"
                  style={{
                    color: '#494949',
                    fontSize: '14px',
                    fontWeight: 'bold',
                    border: '1px solid #E4E4E4'
                  }}
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
              </Box>
            </Box>
          </Form>
        </FormikProvider>
      )}
    </Grid>
  );
}

export default FormSectionSettings;
