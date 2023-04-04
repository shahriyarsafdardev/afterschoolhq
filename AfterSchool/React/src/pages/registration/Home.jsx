import { Box, Button, Grid, Typography, CircularProgress } from '@material-ui/core';
import { useSearchParams, useNavigate, useParams } from 'react-router-dom';
import { useFormik, Form, FormikProvider } from 'formik';
import React, { useEffect, useState } from 'react';
import { useSnackbar } from 'notistack';
import cs from 'classnames';

import { Loader, ProgressBar, ShowQuestion } from 'components';
import RegistrationService from 'sevices/RegistrationService';
import { useRegistration } from 'redux/hooks/index';
import useStyles from './styles';
import apiRoutes from 'routes';
import utils from 'utils';

export default function Home({ newChild = false }) {
  const classes = useStyles();
  const navigate = useNavigate();
  const { scrollToTop, redirect } = utils;
  const { paymentScreenPath } = apiRoutes;
  const { enqueueSnackbar } = useSnackbar();
  const {
    msgs,
    school,
    loading,
    questions,
    loadFormData,
    registrationId,
    questionsGroups,
    answeredQuestions,
    setSchoolFieldValue,
    submitFormSectionData,
    clearMessages,
    activity: { IsFree, RegistrantType }
  } = useRegistration();

  const totalGroups = questionsGroups?.length || 1;
  const [activeGroup, setActiveGroup] = useState(0);
  const [activeGroupId, setActiveGroupId] = useState(0);
  const [saveDisabled, setSaveDisabled] = useState(false);
  const [searchParams, setSearchParams] = useSearchParams();
  const { accountName, activityName, regUid } = useParams();
  const [currentQuestions, setCurrentQuestions] = useState([]);
  const { sectionSkipAble, Name } = questionsGroups[activeGroup] || {};
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

  const buildQueryParams = (index = activeGroup) => `section=${questionsGroups[index].Name}`;

  const handleSection = action => {
    if (action === 'next') {
      setActiveGroupId(questionsGroups[activeGroup + 1].QuestionGroupDataTypeId);
      setSearchParams(buildQueryParams(activeGroup + 1));
      setActiveGroup(activeGroup + 1);
    } else if (action === 'previous') {
      setActiveGroupId(questionsGroups[activeGroup - 1].QuestionGroupDataTypeId);
      setSearchParams(buildQueryParams(activeGroup - 1));
      setActiveGroup(activeGroup - 1);
      clearMessages();
    }
  };

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  useEffect(() => {
    if (registrationId) handleRedirection();
  }, [registrationId]);

  const handleRedirection = () => {
    if (IsFree) {
      const thankyouUrl = `/reactapp/registration/thankyou/${registrationId}`;
      window.history.pushState(null, '', thankyouUrl);
      navigate(thankyouUrl);
    } else redirect(paymentScreenPath(registrationId));
  };

  const secondLastSection = activeGroup === totalGroups - 2;
  const lastSection = activeGroup === totalGroups - 1;
  const initialvalues = initialValues();

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: initialvalues,
    validate: values => validator(values),
    onSubmit: async (values, { resetForm }) => {
      setSaveDisabled(true);
      if (!isEmpty || dirty || secondLastSection) {
        let [schoolLocationID, errors] = [null, null];
        if (
          Name === 'Student Information' &&
          RegistrantType !== 'Adult' &&
          !['', undefined].includes(school.locationName)
        ) {
          await setSchoolFieldValue({ school, noSchool: false }, true).then(({ obj, msgs }) => {
            schoolLocationID = obj;
            errors = msgs;
          });
          if (errors.length) return setSaveDisabled(false);
        }

        await submitFormSectionData(
          formSectionSubmitObject(values, secondLastSection ? -1 : activeGroupId),
          lastSection,
          schoolLocationID,
          newChild ? 1 : 0
        ).then(() => enqueueSnackbar('Submitted.', { variant: 'success' }));
      }
      if (!lastSection && !msgs.length) {
        handleSection('next');
        resetForm();
      } else setSaveDisabled(false);
    }
  });
  const { handleSubmit, dirty, values } = formik;
  const isEmpty = !Object.values(values).some(x => x !== '');
  const skipAble = sectionSkipAble && isEmpty && !dirty;

  useEffect(() => {
    loadFormData(newChild, accountName, activityName, regUid);
  }, [newChild, accountName, activityName]);

  useEffect(() => {
    if (questionsGroups.length)
      setActiveGroupId(questionsGroups[activeGroup].QuestionGroupDataTypeId);
  }, [questionsGroups.length]);

  useEffect(() => {
    if (questions.length) {
      let QuestionGroupDataTypeId = '';
      const paramsQuery = buildQueryParams();
      if (searchParams.get('section')) {
        const groupIndex = questionsGroups.findIndex(
          section => section.Name === searchParams.get('section')
        );
        if (groupIndex < 0) return setSearchParams(paramsQuery);
        QuestionGroupDataTypeId = questionsGroups[groupIndex].QuestionGroupDataTypeId;
        setActiveGroupId(QuestionGroupDataTypeId);
        setActiveGroup(groupIndex);
      } else setSearchParams(paramsQuery);
    }
  }, [questions.length, searchParams.get('section')]);

  useEffect(() => {
    setSaveDisabled(false);
    if (questions.length) {
      const { QuestionGroupDataTypeId, Name } = questionsGroups[activeGroup];
      setCurrentQuestions(groupedQuestions(QuestionGroupDataTypeId, Name));
    }
    scrollToTop();
  }, [activeGroup, activeGroupId, questions]);

  return (
    <FormikProvider value={formik}>
      <Form autoComplete="off" noValidate onSubmit={handleSubmit} className={classes.form}>
        {loading ? (
          <Loader />
        ) : (
          <Box className={classes.outerWrapper}>
            <ProgressBar activeGroup={activeGroup} totalGroups={totalGroups} />
            <Box className={classes.formWrapper}>
              <Box className={classes.formInnerWrapper}>
                <Typography style={{ padding: '30px 0px' }} variant="h6" color="primary">
                  {Name}
                </Typography>
                <Grid container className={cs(classes.justifyContentCenter, classes.leftPadding)}>
                  <Grid className={classes.innerContainer} item xs={12} md={12} lg={12}>
                    {currentQuestions?.map(question =>
                      ShowQuestion({ currentQuestion: question, formik, currentQuestions })
                    )}
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
                      key={activeGroup}
                    >
                      {saveDisabled ? (
                        <>
                          {'Loading . . .'} <CircularProgress size={'20px'} />
                        </>
                      ) : (
                        saveButtonText(lastSection, RegistrantType, IsFree, skipAble)
                      )}
                    </Button>
                  </Box>
                </Box>
              </Box>
            </Box>
          </Box>
        )}
      </Form>
    </FormikProvider>
  );
}
