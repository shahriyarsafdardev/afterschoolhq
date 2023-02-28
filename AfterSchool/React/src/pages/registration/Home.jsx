import { Box, Button, Grid, Typography, CircularProgress } from '@material-ui/core';
import { useFormik, Form, FormikProvider } from 'formik';
import React, { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useSnackbar } from 'notistack';
import cs from 'classnames';

import { NavBar, Loader, CheckBox, TextField, ProgressBar, ApiTextField } from 'components';
import RegistrationService from 'sevices/RegistrationService';
import { useUser, useRegistration } from 'redux/hooks/index';
import questionType from 'constants/questionType';
import apiRoutes from 'routes/apiRoutes';
import useStyles from './styles';
import utils from 'utils';
import {
  DateQuestion,
  RadioQuestion,
  TextAreaQuestion,
  DropdownQuestion
} from 'components/Questions';

export default function Home() {
  const classes = useStyles();
  const { loadUser } = useUser();
  const { scrollToTop, redirect } = utils;
  const { enqueueSnackbar } = useSnackbar();
  const { paymentScreenPath, thankyouScreenPath } = apiRoutes;
  const {
    msgs,
    loading,
    questions,
    loadFormData,
    registrationId,
    questionsGroups,
    answeredQuestions,
    submitFormSectionData,
    activity: { IsFree, RegistrantType }
  } = useRegistration();

  const totalGroups = questionsGroups?.length || 1;
  const [activeGroup, setActiveGroup] = useState(0);
  const [activeGroupId, setActiveGroupId] = useState(0);
  const [saveDisabled, setSaveDisabled] = useState(false);
  const [searchParams, setSearchParams] = useSearchParams();
  const [currentQuestions, setCurrentQuestions] = useState([]);
  const registrationService = new RegistrationService(
    questions,
    currentQuestions,
    answeredQuestions
  );
  const {
    validator,
    initialValues,
    saveButtonText,
    groupedQuestions,
    formSectionSubmitObject
  } = registrationService;

  const handleSection = action => {
    if (action === 'next') {
      setActiveGroupId(questionsGroups[activeGroup + 1].QuestionGroupDataTypeId);
      setSearchParams(`section=${questionsGroups[activeGroup + 1].Name}`);
      setActiveGroup(activeGroup + 1);
    } else if (action === 'previous') {
      setActiveGroupId(questionsGroups[activeGroup - 1].QuestionGroupDataTypeId);
      setSearchParams(`section=${questionsGroups[activeGroup - 1].Name}`);
      setActiveGroup(activeGroup - 1);
    }
  };

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  useEffect(() => {
    if (registrationId) handleRedirection();
  }, [registrationId]);

  const handleRedirection = () =>
    redirect(IsFree ? thankyouScreenPath(registrationId) : paymentScreenPath(registrationId));

  const lastSection = activeGroup === totalGroups - 1;
  const initialvalues = initialValues();
  const formik = useFormik({
    enableReinitialize: true,
    initialValues: initialvalues,
    validate: values => validator(values),
    onSubmit: async (values, { resetForm }) => {
      setSaveDisabled(true);
      await submitFormSectionData(formSectionSubmitObject(values, activeGroupId), lastSection);
      enqueueSnackbar('Submitted.', { variant: 'success' });
      if (!lastSection) handleSection('next');
      resetForm();
    }
  });

  const { handleSubmit } = formik;

  useEffect(() => {
    loadUser();
    loadFormData();
  }, []);

  useEffect(() => {
    if (questionsGroups.length)
      setActiveGroupId(questionsGroups[activeGroup].QuestionGroupDataTypeId);
  }, [questionsGroups.length]);

  useEffect(() => {
    if (questions.length) {
      let QuestionGroupDataTypeId = '';
      if (searchParams.get('section')) {
        const groupIndex = questionsGroups.findIndex(
          section => section.Name === searchParams.get('section')
        );
        if (groupIndex < 0) return setSearchParams(`section=${questionsGroups[activeGroup].Name}`);
        QuestionGroupDataTypeId = questionsGroups[groupIndex].QuestionGroupDataTypeId;
        setActiveGroupId(QuestionGroupDataTypeId);
        setActiveGroup(groupIndex);
      } else setSearchParams(`section=${questionsGroups[activeGroup].Name}`);
    }
  }, [questions.length, searchParams.get('section')]);

  useEffect(() => {
    setSaveDisabled(false);
    if (questions.length)
      setCurrentQuestions(groupedQuestions(questionsGroups[activeGroup]?.QuestionGroupDataTypeId));
    scrollToTop();
  }, [activeGroup, activeGroupId, questions]);

  const showQuestion = currentQuestion => {
    const { QuestionTypeId } = currentQuestion;

    switch (QuestionTypeId) {
      case questionType.text:
        return ['School', 'Street Address'].includes(currentQuestion.Text) ? (
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
                          type="submit"
                          disabled={saveDisabled}
                        >
                          {saveDisabled ? (
                            <>
                              {'Loading . . .'} <CircularProgress size={'20px'} />
                            </>
                          ) : (
                            saveButtonText(lastSection, RegistrantType, IsFree)
                          )}
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
