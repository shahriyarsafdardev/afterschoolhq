import { useSearchParams, useNavigate, useParams } from 'react-router-dom';
import React, { useEffect, useState } from 'react';
import { useSnackbar } from 'notistack';
import { useFormik } from 'formik';

import { useVolunteerRegistration } from 'redux/hooks';
import RegistrationService from 'sevices/RegistrationService';
import RegistrationForm from 'pages/shared/RegistrationForm';
import apiRoutes from 'routes';
import utils from 'utils';

export default function VolunteerRegistration() {
  const { scrollToTop } = utils;
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

  const buildQueryParams = (index = activeGroup) =>
    `section=${encodeURIComponent(questionsGroups[index].Name)}`;

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
    // validate: values => validator(values, genderRestrictions),
    onSubmit: async (values, { resetForm }) => {
      setSaveDisabled(true);
      let errorMsgs = [];
      if (!isEmpty || dirty || secondLastSection) {
        await submitFormSectionData(
          formSectionSubmitObject(values, secondLastSection ? -1 : activeGroupId),
          lastSection,
          accountName,
          activityName,
          regUid
        ).then(errors => {
          if (!errors.length) enqueueSnackbar('Submitted.', { variant: 'success' });
          else errorMsgs = errors;
        });
      }
      if (!lastSection && !errorMsgs.length) {
        handleSection('next');
        resetForm();
      } else setSaveDisabled(false);
    }
  });

  const { handleSubmit, dirty, values } = formik;
  const isEmpty = !Object.values(values).some(x => x !== '');
  const skipAble = sectionSkipAble && isEmpty && !dirty;

  useEffect(() => {
    loadFormData(accountName, activityName, regUid);
  }, [accountName, activityName, regUid]);

  useEffect(() => {
    if (questionsGroups?.length)
      setActiveGroupId(questionsGroups[activeGroup].QuestionGroupDataTypeId);
  }, [questionsGroups?.length]);

  useEffect(() => {
    if (questions?.length) {
      let QuestionGroupDataTypeId = '';
      const paramsQuery = buildQueryParams();
      if (searchParams.get('section')) {
        const groupIndex = questionsGroups.findIndex(
          section => section.Name === decodeURI(searchParams.get('section'))
        );
        if (groupIndex < 0) return setSearchParams(paramsQuery);
        QuestionGroupDataTypeId = questionsGroups[groupIndex].QuestionGroupDataTypeId;
        setActiveGroupId(QuestionGroupDataTypeId);
        setActiveGroup(groupIndex);
      } else setSearchParams(paramsQuery);
    }
  }, [questions?.length, searchParams.get('section')]);

  useEffect(() => {
    setSaveDisabled(false);
    if (questions?.length) {
      const { QuestionGroupDataTypeId, Name } = questionsGroups[activeGroup];
      setCurrentQuestions(groupedQuestions(QuestionGroupDataTypeId, Name));
    }
    scrollToTop();
  }, [activeGroup, activeGroupId, questions]);

  const exitHandler = () => {
    exit(regUid, ActivityUrl);
  };

  return (
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
    ></RegistrationForm>
  );
}
