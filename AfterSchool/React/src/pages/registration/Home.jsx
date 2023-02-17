import React, { useEffect, useState } from 'react';
import { Box, Button, Grid, Typography } from '@material-ui/core';
import { useFormik, Form, FormikProvider } from 'formik';

import TextField from 'components/TextField/TextField';
import ProgressBar from 'components/ProgressBar/ProgressBar';
import NavBar from 'components/NavBar/NavBar';
import Loader from 'components/Loader/Loader';
import CheckBox from 'components/CheckBox';
import { useUser, useRegistration } from 'redux/hooks/index';
import questionType from 'constants/questionType';
import DateQuestion from 'components/Questions/DateQuestion';
import DropdownQuestion from 'components/Questions/DropdownQuestion';
import RadioQuestion from 'components/Questions/RadioQuestion';
import TextAreaQuestion from 'components/Questions/TextAreaQuestion';
import ApiTextField from 'components/ApiTextField/ApiTextField';
import Validations from 'constants/regex';

import useStyles from './styles';

export default function Home() {
  const classes = useStyles();
  const { loadUser } = useUser();
  const {
    loading,
    questions,
    questionsGroups,
    answeredQuestions,
    loadFormData,
    submitFormSection
  } = useRegistration();
  const { emailValidationRegex, phoneValidationRegex } = Validations;

  const [activeGroup, setActiveGroup] = useState(0);
  const [activeGroupId, setActiveGroupId] = useState(
    questionsGroups ? questionsGroups[0]?.QuestionGroupDataTypeId : 0
  );
  const [currentQuestions, setCurrentQuestions] = useState([]);
  const totalGroups = questionsGroups?.length || 1;

  const handleSection = action => {
    if (action === 'next') {
      setActiveGroupId(questionsGroups[activeGroup + 1].QuestionGroupDataTypeId);
      setActiveGroup(activeGroup + 1);
    } else if (action === 'previous') {
      setActiveGroupId(questionsGroups[activeGroup - 1].QuestionGroupDataTypeId);
      setActiveGroup(activeGroup - 1);
    }
  };

  const submittedAnswer = id =>
    answeredQuestions.find(q => q.QuestionDataTypeId === id)?.Text || '';

  const questionObjectFindById = id => currentQuestions.find(q => q.ID === Number(id)) || {};

  const answerObject = (key, values) => {
    if (!values[key]) return undefined;
    const question = questionObjectFindById(key);
    return {
      QuestionDataTypeId: question.QuestionDataTypeId,
      QuestionText: question.Text,
      Text: values[key]
    };
  };

  const formSectionSubmitObject = values => {
    const answers = [];
    Object.keys(values).forEach(key => {
      const answer = answerObject(key, values);
      if (answer) answers.push(answer);
    });
    answeredQuestions.forEach(answer => {
      if (!Object.keys(values).includes(answer.QuestionDataTypeId)) {
        answers.push(answer);
      }
    });
    return answers;
  };

  const validator = values => {
    const errors = {};
    currentQuestions?.forEach(question => {
      if (
        question?.IsRequired &&
        question.QuestionTypeId === questionType.email &&
        values[question.ID]
      ) {
        if (!emailValidationRegex.test(values[question.ID])) {
          errors[question.ID] = 'Please enter a valid email address';
        }
      } else if (question?.IsRequired && !values[question.ID]) {
        errors[question.ID] = `${question.Text} is required`;
      }

      if (values[question.ID]) {
        if (question.QuestionTypeId === questionType.email) {
          if (!emailValidationRegex.test(values[question.ID])) {
            errors[question.ID] = 'Please enter a valid email address';
          }
        }
        if (question.QuestionTypeId === questionType.text && question.Text.includes('Phone')) {
          if (!phoneValidationRegex.test(values[question.ID])) {
            errors[question.ID] = 'Please enter a valid phone number';
          }
        }
      }
    });
    return errors;
  };

  const initialvalues = {};
  currentQuestions?.forEach(question => {
    initialvalues[question.ID] = submittedAnswer(question.QuestionDataTypeId);
  });

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: initialvalues,
    validate: values => validator(values),
    onSubmit: async (values, { resetForm }) => {
      await submitFormSection(formSectionSubmitObject(values));
      handleSection('next');
      resetForm();
    }
  });

  const { handleSubmit } = formik;

  useEffect(() => {
    loadUser();
    loadFormData();
  }, []);

  useEffect(() => {
    if (questions) {
      const groupedQuestions = questions.filter(
        question =>
          question.FormQuestionGroupQuestionGroupDataTypeId ===
          questionsGroups[activeGroup].QuestionGroupDataTypeId
      );
      setCurrentQuestions(groupedQuestions);
    }
  }, [activeGroup, activeGroupId, questions]);

  const showQuestion = currentQuestion => {
    const { QuestionTypeId } = currentQuestion;

    switch (QuestionTypeId) {
      case questionType.text:
        return currentQuestion.Text === 'Street Address' ? (
          <ApiTextField question={currentQuestion} formik={formik} />
        ) : (
          <TextField question={currentQuestion} formik={formik} />
        );
      case questionType.email:
        return <TextField question={currentQuestion} formik={formik} />;
      case questionType.date:
        return <DateQuestion question={currentQuestion} formik={formik} />;
      case questionType.dropdown:
        return <DropdownQuestion question={currentQuestion} formik={formik} />;
      case questionType.radioList:
        return <RadioQuestion question={currentQuestion} formik={formik} />;
      case questionType.textArea:
        return <TextAreaQuestion question={currentQuestion} formik={formik} />;
      case questionType.checkBox:
        return (
          <CheckBox
            option={{ key: 1, value: 'Test Value' }}
            click={() => {}}
            checked
            formik={formik}
          />
        );
      default:
        return <TextField question={currentQuestion} formik={formik} />;
    }
  };

  return (
    <FormikProvider value={formik}>
      <Form autoComplete="off" noValidate onSubmit={handleSubmit}>
        <Box className={classes.home}>
          {loading ? (
            <Loader />
          ) : (
            <>
              <NavBar />
              <Box className={classes.outerWrapper}>
                <ProgressBar activeGroup={activeGroup} totalGroups={totalGroups} />
                <Box className={classes.formWrapper}>
                  <Box className={classes.formInnerWrapper}>
                    <Typography
                      style={{
                        paddingTop: '30px',
                        fontSize: 18,
                        color: '#555555'
                      }}
                      variant="h6"
                      color="secondary"
                    >
                      Please enter your information below to register
                    </Typography>
                    <Typography style={{ padding: '30px 0px' }} variant="h6" color="primary">
                      {questionsGroups[activeGroup].Name}
                    </Typography>
                    <Grid container className={classes.justifyContentCenter}>
                      <Grid className={classes.innerContainer} item xs={12} md={12} lg={12}>
                        {currentQuestions?.map(question => showQuestion(question))}
                      </Grid>
                    </Grid>
                    <Box className={classes.buttonWrapper}>
                      <Button
                        color="primary"
                        variant="outlined"
                        className={classes.backButton}
                        disabled={!activeGroup > 0}
                        onClick={() => handleSection('previous')}
                      >
                        Back
                      </Button>
                      <Button
                        color="primary"
                        className={classes.saveButton}
                        variant="contained"
                        disabled={activeGroup === totalGroups - 1}
                        type="submit"
                      >
                        {activeGroup === totalGroups - 1
                          ? 'Confirm Registration'
                          : 'Save & Continue'}
                      </Button>
                    </Box>
                  </Box>
                </Box>
              </Box>
            </>
          )}
        </Box>
      </Form>
    </FormikProvider>
  );
}
