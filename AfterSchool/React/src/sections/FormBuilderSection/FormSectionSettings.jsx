import cs from 'classnames';
import React, { useEffect, useState } from 'react';
import { useFormik, FormikProvider, Form } from 'formik';
import { useSearchParams, useParams } from 'react-router-dom';
import { Grid, Typography, TextField, Box, Button } from '@material-ui/core';
import { DragDropContext, Droppable, Draggable } from 'react-beautiful-dnd';
import DragIndicatorIcon from '@material-ui/icons/DragIndicator';

import useStyles from './styles';
import { Loader } from 'components';
import { useFormBuilder } from 'redux/hooks';
import RenderQuestion from './RenderQuestion';
import RenderCheckbox from './RenderCheckbox';
import { toInteger } from 'lodash';
import { newQuestion } from './helper';

function FormSectionSettings() {
  const classes = useStyles();
  const { formId } = useParams();
  const initialState = {
    All: false,
    Parent: false,
    Student: false,
    Adult: false
  };

  const [searchParams, setSearchParams] = useSearchParams();
  const questionGroup = searchParams.get('qg');

  const {
    loadSectionData,
    submitFormSectionData,
    questionsGroups,
    questions,
    sectionLoading
  } = useFormBuilder();
  const currentSection = questionsGroups.find(qg => qg.QuestionGroupDataTypeId == questionGroup);
  const sectionName = currentSection?.Name || '';

  useEffect(() => {
    loadSectionData(formId, questionGroup);
  }, [formId, questionGroup]);

  const initialValues = {};
  questions.map(question => (initialValues[question.DisplayOrder] = question));

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: initialValues,
    // validationSchema,
    validate: values => {
      console.log(values, 'validate successfully');
      return {};
    },
    onSubmit: async (values, ...props) => {
      const formQuestions = Object.keys(values).map(key => values[key]);
      await submitFormSectionData({
        FormSettings: { FormId: formId },
        FormQuestionGroup: currentSection,
        FormQuestion: formQuestions
      });
      console.log(values, props, 'submitted successfully');
    }
  });
  const { handleSubmit, values, setFieldValue, setValues } = formik;

  const addNewQuestionHandler = index => {
    const newDisplayOrder = toInteger(
      index || values[Object.keys(values).at(-1)]?.DisplayOrder || -1
    );
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

    setValues({ ...newValues });
  };

  const handleCopy = key => {
    const { [key]: copiedValue, ...restValues } = values;

    const newValues = Object.keys(values).reduce((acc, k) => {
      const newKey = Number(k) > Number(key) ? Number(k) + 1 : k;
      acc[newKey] = { ...values[k], DisplayOrder: newKey };
      return acc;
    }, {});

    newValues[Number(key) + 1] = { ...copiedValue, ID: 0, DisplayOrder: Number(key) + 1 };
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

    // const { [source.index]: copiedValue, ...restValues } = values;
    // moveObjectInFormikState(source.index, destination.index);
    console.log(source, destination);

    // const temp = [...options];
    // const [sourceValue] = temp.splice(source.index, 1);
    // temp.splice(destination.index, 0, sourceValue);
    // setOptions(temp);
  };

  //////
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
        <Box className={classes.displayFlex}>
          <Typography variant="body1" className={classes.applyAllRow}>
            Show For:
          </Typography>
          {Object.keys(showFor).map(key => (
            <Box key={key} className={cs(classes.displayFlex, classes.applyToAllControlsWrapper)}>
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
            </Box>
          ))}
        </Box>
        <Box className={classes.displayFlex}>
          <Typography variant="body1" className={classes.applyAllRow}>
            Required For:
          </Typography>
          {Object.keys(requiredFor).map(key => (
            <Box key={key} className={cs(classes.displayFlex, classes.applyToAllControlsWrapper)}>
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
            </Box>
          ))}
        </Box>
      </Box>
    );
  };

  return (
    <Grid item lg={9} xs={12} sm={6}>
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
                fullWidth
                variant="outlined"
                value={sectionName}
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
                          {Object.keys(values).map((key, index) => (
                            <Draggable draggableId={`question-${key}`} index={key} key={key}>
                              {(provided, snapshot) => (
                                <Box
                                  ref={provided.innerRef}
                                  {...provided.draggableProps}
                                  //   {...provided.dragHandleProps}
                                  className={
                                    snapshot.isDragging
                                      ? classes.whiteBorder
                                      : classes.transparentBorder
                                  }
                                >
                                  <span {...provided.dragHandleProps}>
                                    <DragIndicatorIcon style={{ float: 'left' }} />
                                  </span>
                                  <RenderQuestion
                                    index={key}
                                    key={`${key}-${values[key]?.ID}${reRender}`}
                                    value={values[key]}
                                    setFieldValue={setFieldValue}
                                    handleDelete={handleDelete}
                                    handleCopy={handleCopy}
                                    handleAddNewQuestion={addNewQuestionHandler}
                                  />
                                </Box>
                              )}
                            </Draggable>
                          ))}
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
                  //   type="submit"
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
                  //   className={classes.backButton}
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
