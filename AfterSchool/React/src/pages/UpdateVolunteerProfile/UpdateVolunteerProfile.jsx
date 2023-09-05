import { useFormik } from 'formik';
import { useSnackbar } from 'notistack';
import React, { useEffect, useState } from 'react';
import { useSearchParams, useParams } from 'react-router-dom';

import { useErrorMessages, useUser, useVolunteerProfile } from 'redux/hooks/index';
import RegistrationService from 'sevices/RegistrationService';
import RegistrationForm from 'pages/shared/RegistrationForm';
import apiRoutes from 'routes';
import utils from 'utils';

export default function UpdateVolunteerProfile() {
  const { loadUser } = useUser();
  const { volunteerAccountUrl } = apiRoutes;
  const { id: userUid } = useParams();
  const { scrollToTop, redirect } = utils;
  const { enqueueSnackbar } = useSnackbar();
  const { msgs } = useErrorMessages();
  const {
    loaded,
    loading,
    questions,
    loadFormData,
    clearMessages,
    questionsGroups,
    answeredQuestions,
    submitFormSectionData
  } = useVolunteerProfile();

  const totalGroups = questionsGroups?.length || 1;
  const [activeGroup, setActiveGroup] = useState(0);
  const [activeGroupId, setActiveGroupId] = useState(0);
  const [saveDisabled, setSaveDisabled] = useState(false);
  const [searchParams, setSearchParams] = useSearchParams();
  const [currentQuestions, setCurrentQuestions] = useState([]);

  const { Name } = questionsGroups?.length ? questionsGroups[activeGroup] : {};
  const registrationService = new RegistrationService(
    questions,
    currentQuestions,
    answeredQuestions
  );
  const {
    validator,
    initialValues,
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

  const handleRedirection = () => {
    loaded();
    redirect(volunteerAccountUrl);
  };

  const lastSection = activeGroup === totalGroups - 1;
  const initialvalues = initialValues();

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: initialvalues,
    validate: values => validator(values),
    onSubmit: async (values, { resetForm }) => {
      if (dirty) {
        setSaveDisabled(true);

        await submitFormSectionData(
          formSectionSubmitObject(values, activeGroupId),
          userUid
        ).then(() => enqueueSnackbar('Submitted.', { variant: 'success' }));
      }
      if (!msgs.length) {
        if (!lastSection) {
          handleSection('next');
          resetForm();
        } else handleRedirection();
      }
      setSaveDisabled(false);
    }
  });

  const { handleSubmit, dirty } = formik;

  useEffect(() => {
    loadUser();
    loadFormData(userUid);
  }, [userUid]);

  useEffect(() => {
    if (questionsGroups?.length)
      setActiveGroupId(questionsGroups[activeGroup].QuestionGroupDataTypeId);
  }, [questionsGroups?.length]);

  useEffect(() => {
    if (questions.length) {
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
    if (questions.length) {
      const { QuestionGroupDataTypeId, Name } = questionsGroups[activeGroup];
      setCurrentQuestions(groupedQuestions(QuestionGroupDataTypeId, Name));
    }
    scrollToTop();
  }, [activeGroup, activeGroupId, questions]);

  const saveButtonText = () => (dirty ? 'Save' : 'Skip');

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
      isUpdating={true}
      handleRedirection={handleRedirection}
    ></RegistrationForm>
  );
}
