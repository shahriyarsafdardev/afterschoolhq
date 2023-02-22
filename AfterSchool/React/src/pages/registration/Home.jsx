import React, { useEffect, useState } from 'react';
import { Box, Button, Grid, Typography } from '@material-ui/core';
import cs from 'classnames';
import { useFormik, Form, FormikProvider } from 'formik';

import RegistrationService from 'sevices/RegistrationService';
import apiRoutes from 'routes/apiRoutes';
import utils from 'utils';
import { useUser, useRegistration } from 'redux/hooks/index';
import { TextField, ProgressBar, NavBar, Loader, CheckBox, ApiTextField } from 'components';
import {
  DateQuestion,
  DropdownQuestion,
  RadioQuestion,
  TextAreaQuestion
} from 'components/Questions';

import questionType from 'constants/questionType';

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
    submitFormSection,
    isActivityPaid,
    isParent
  } = useRegistration();
  const { scrollToTop, redirect } = utils;
  const { paymentScreenPath, thankyouScreenPath } = apiRoutes;

  const [activeGroup, setActiveGroup] = useState(10);
  const [activeGroupId, setActiveGroupId] = useState(0);
  const [currentQuestions, setCurrentQuestions] = useState([]);
  const totalGroups = questionsGroups?.length || 1;
  const registrationService = new RegistrationService(
    questions,
    answeredQuestions,
    currentQuestions
  );
  const {
    initialValues,
    validator,
    formSectionSubmitObject,
    groupedQuestions
  } = registrationService;

  const handleSection = action => {
    if (action === 'next') {
      setActiveGroupId(questionsGroups[activeGroup + 1].QuestionGroupDataTypeId);
      setActiveGroup(activeGroup + 1);
    } else if (action === 'previous') {
      setActiveGroupId(questionsGroups[activeGroup - 1].QuestionGroupDataTypeId);
      setActiveGroup(activeGroup - 1);
    }
  };

  const handleRedirection = () => {
    redirect(isActivityPaid ? paymentScreenPath : thankyouScreenPath);
  };

  const initialvalues = initialValues();

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: initialvalues,
    validate: values => validator(values),
    onSubmit: async (values, { resetForm }) => {
      await submitFormSection(formSectionSubmitObject(values, activeGroupId));
      if (activeGroup === totalGroups - 1) handleRedirection();
      else handleSection('next');
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
    if (questions)
      setCurrentQuestions(groupedQuestions(questionsGroups[activeGroup]?.QuestionGroupDataTypeId));
    scrollToTop();
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
                    <Box className={classes.buttonOuterWrapper}>
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
                            ? isParent
                              ? isActivityPaid
                                ? 'E-Sign and Continue to Payment '
                                : 'Confirm Registration'
                              : 'Confirm'
                            : 'Save & Continue'}
                        </Button>
                      </Box>
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
