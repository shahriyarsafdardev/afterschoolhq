import { useSearchParams, useNavigate, useParams } from 'react-router-dom';
import React, { useEffect, useState } from 'react';
import { Box } from '@material-ui/core';
import { useSnackbar } from 'notistack';
import { useFormik } from 'formik';

import { handleStatusAndRedirection, setSectionIndexToStorage } from 'pages/shared/helper';
import RegistrationService from 'sevices/RegistrationService';
import RegistrationForm from 'pages/shared/RegistrationForm';
import { checkEmptyValues } from 'pages/shared/helper';
import { useVolunteerRegistration } from 'redux/hooks';
import SidebarSection from 'pages/shared/Sidebar';
import apiRoutes from 'routes';

export default function VolunteerRegistration() {
  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();
  const { volunteerThankYouUrl } = apiRoutes;
  const { accountName, activityName, regUid } = useParams();
  const {
    exit,
    loading,
    questions,
    loadFormData,
    clearMessages,
    registrationId,
    questionsGroups,
    answeredQuestions,
    setStatusCompleted,
    submitFormSectionData,
    activity: { IsFree, RegistrantType, IsGenderRestricted, EnforcedGender, ActivityUrl }
  } = useVolunteerRegistration();

  const totalGroups = questionsGroups?.length || 1;
  const [activeGroup, setActiveGroup] = useState(0);
  const [activeGroupId, setActiveGroupId] = useState(0);
  const [saveDisabled, setSaveDisabled] = useState(false);
  const [searchParams, setSearchParams] = useSearchParams();
  const [currentQuestions, setCurrentQuestions] = useState([]);
  const { sectionSkipAble, Name } = questionsGroups[activeGroup] || {};
  const stateSetters = {
    setActiveGroup,
    setActiveGroupId,
    setSearchParams,
    setSaveDisabled,
    setCurrentQuestions
  };
  const registrationService = new RegistrationService(
    questions,
    currentQuestions,
    answeredQuestions,
    false,
    stateSetters
  );
  const {
    validator,
    initialValues,
    saveButtonText,
    groupedQuestions,
    formSectionSubmitObject,
    handleSectionNavigation,
    setCurrentSectionParams,
    setCurrentSectionQuestions
  } = registrationService;

  const handleSection = action =>
    handleSectionNavigation(action, activeGroup, clearMessages, questionsGroups);

  useEffect(() => {
    if (registrationId) handleRedirection();
  }, [registrationId]);

  const handleRedirection = () => {
    const thankyouUrl = volunteerThankYouUrl(registrationId);
    window.history.pushState(null, '', thankyouUrl);
    navigate(thankyouUrl);
  };

  const genderRestrictions = {
    enforcedGender: EnforcedGender,
    isGenderRestricted: IsGenderRestricted
  };
  const secondLastSection = activeGroup === totalGroups - 2;
  const lastSection = activeGroup === totalGroups - 1;
  const initialvalues = initialValues();

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: initialvalues,
    validate: values => validator(values, genderRestrictions),
    onSubmit: async (values, { resetForm }) => {
      setSaveDisabled(true);
      let errorMsgs = [];
      const groupId = secondLastSection ? -1 : activeGroupId;
      if (!isEmpty || dirty || secondLastSection) {
        await submitFormSectionData(
          formSectionSubmitObject(values, groupId),
          lastSection,
          accountName,
          activityName,
          regUid
        ).then(errors => {
          if (!errors.length) {
            if (lastSection) localStorage.removeItem('lastCompletedSectionId');
            enqueueSnackbar('Submitted.', { variant: 'success' });
          } else errorMsgs = errors;
        });
      }
      if (!lastSection && !errorMsgs.length) {
        handleStatusAndRedirection(setStatusCompleted, handleSection, resetForm, activeGroup);
      } else setSaveDisabled(false);
      if (!lastSection) setSectionIndexToStorage(activeGroup);
    }
  });

  const { handleSubmit, dirty, values } = formik;
  const isEmpty = checkEmptyValues(values);
  const skipAble = sectionSkipAble && isEmpty && !dirty;

  useEffect(() => {
    loadFormData(accountName, activityName, regUid);
  }, [accountName, activityName, regUid]);

  useEffect(() => {
    if (questionsGroups?.length)
      setActiveGroupId(questionsGroups[activeGroup].QuestionGroupDataTypeId);
  }, [questionsGroups?.length]);

  useEffect(() => {
    setCurrentSectionParams(questions, activeGroup, searchParams, questionsGroups);
  }, [questions?.length, searchParams.get('section')]);

  useEffect(() => {
    setCurrentSectionQuestions(questions, activeGroup, questionsGroups, groupedQuestions);
  }, [activeGroup, activeGroupId, questions]);

  const exitHandler = () => {
    exit(regUid, ActivityUrl);
  };

  return (
    <Box style={{ position: 'relative' }}>
      <RegistrationForm
        loading={loading}
        totalGroups={totalGroups}
        activeGroup={activeGroup}
        saveDisabled={saveDisabled}
        currentQuestions={currentQuestions}
        Name={Name}
        saveButtonText={saveButtonText}
        handleSection={handleSection}
        lastSection={lastSection}
        formik={formik}
        handleSubmit={handleSubmit}
        skipAble={skipAble}
        registrantType={RegistrantType}
        isFree={IsFree}
        firstSection={activeGroup === 0}
        exitHandler={exitHandler}
        showProgressBar={false}
      ></RegistrationForm>
      {!loading && <SidebarSection questionsGroups={questionsGroups} />}
    </Box>
  );
}
