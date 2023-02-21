import React, { useEffect, useState } from 'react';
import { Box, Button, Grid, Typography } from '@material-ui/core';
import cs from 'classnames';
import { useFormik, Form, FormikProvider } from 'formik';

import { useUser, useRegistration } from 'redux/hooks/index';
import { TextField, ProgressBar, NavBar, Loader, CheckBox, ApiTextField } from 'components';
import {
  DateQuestion,
  DropdownQuestion,
  RadioQuestion,
  TextAreaQuestion
} from 'components/Questions';

import questionType from 'constants/questionType';
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

  const [activeGroup, setActiveGroup] = useState(10);
  const [activeGroupId, setActiveGroupId] = useState(0);
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

  const submittedAnswer = (id, text) =>
    answeredQuestions.find(q => q.QuestionDataTypeId === id && q.QuestionText === text)?.Text || '';

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
    const answerQuery = {};
    const answers = [];
    const keys = Object.keys(values);
    const getQuestionID = answer =>
      questions.find(q => q.QuestionDataTypeId === answer.QuestionDataTypeId)?.ID || 0;
    function addToAnswerQuery(ID, text) {
      answerQuery[`question-${ID}`] = text;
    }
    function composeAnswers(answer, ID, shouldCompose) {
      if (!shouldCompose) return;
      answers.push(answer);
      addToAnswerQuery(ID, answer.Text);
    }
    answeredQuestions.forEach(answer => {
      composeAnswers(answer, getQuestionID(answer), !keys.includes(answer.QuestionDataTypeId));
    });
    keys.forEach(key => {
      const answer = answerObject(key, values);
      composeAnswers(answer, key, answer);
    });
    return { answers, answerQuery: { answerQuery, groupId: activeGroupId } };
  };

  const validator = values => {
    const errors = {};
    currentQuestions?.forEach(question => {
      const { ID, IsRequired, QuestionTypeId, Text } = question;
      const valueOfCurrentQuestion = values[ID];
      if (IsRequired && QuestionTypeId === questionType.email && valueOfCurrentQuestion) {
        if (!emailValidationRegex.test(valueOfCurrentQuestion)) {
          errors[ID] = 'Please enter a valid email address';
        }
      } else if (IsRequired && !valueOfCurrentQuestion) {
        errors[ID] = `${Text} is required`;
      }

      if (valueOfCurrentQuestion) {
        if (QuestionTypeId === questionType.email) {
          if (!emailValidationRegex.test(valueOfCurrentQuestion)) {
            errors[ID] = 'Please enter a valid email address';
          }
        }
        if (QuestionTypeId === questionType.text && Text.includes('Phone')) {
          if (!phoneValidationRegex.test(valueOfCurrentQuestion)) {
            errors[ID] = 'Please enter a valid phone number';
          }
        }
      }
    });
    return errors;
  };

  const initialvalues = {};
  currentQuestions?.forEach(question => {
    initialvalues[question.ID] = submittedAnswer(question.QuestionDataTypeId, question.Text);
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
    if (questionsGroups.length) setActiveGroupId(questionsGroups[0].QuestionGroupDataTypeId);
  }, [questionsGroups.length]);

  useEffect(() => {
    if (questions) {
      let groupedQuestions = questions.filter(
        question =>
          question.FormQuestionGroupQuestionGroupDataTypeId ===
          questionsGroups[activeGroup].QuestionGroupDataTypeId
      );
      groupedQuestions = groupedQuestions.sort((q1, q2) => q1.DispalayOrder - q2.DispalayOrder);
      setCurrentQuestions(groupedQuestions);
    }
    window.scrollTo(0, 0);
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
        return <CheckBox question={currentQuestion} formik={formik} />;
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
                    <Grid
                      container
                      className={cs(classes.justifyContentCenter, classes.leftPadding)}
                    >
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
                        // disabled={activeGroup === totalGroups - 1}
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
