import { useFormik } from 'formik';
import { useSnackbar } from 'notistack';
import React, { useEffect, useState } from 'react';
import { useSearchParams, useParams } from 'react-router-dom';

import { completeLoadingAndRedirect } from 'pages/shared/helper';
import { useUser, useVolunteerProfile } from 'redux/hooks/index';
import RegistrationService from 'sevices/RegistrationService';
import RegistrationForm from 'pages/shared/RegistrationForm';
import apiRoutes from 'routes';

export default function UpdateVolunteerProfile() {
  const { loadUser } = useUser();
  const { volunteerAccountUrl } = apiRoutes;
  const { id: userUid } = useParams();
  const { enqueueSnackbar } = useSnackbar();
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
  const stateSetters = {
    setActiveGroup,
    setSearchParams,
    setActiveGroupId,
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
    groupedQuestions,
    formSectionSubmitObject,
    handleSectionNavigation,
    setCurrentSectionParams,
    setCurrentSectionQuestions
  } = registrationService;

  const handleSection = action =>
    handleSectionNavigation(action, activeGroup, clearMessages, questionsGroups);

  const handleRedirection = () => completeLoadingAndRedirect(loaded, volunteerAccountUrl);

  const lastSection = activeGroup === totalGroups - 1;
  const initialvalues = initialValues();

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: initialvalues,
    validate: values => validator(values),
    onSubmit: async (values, { resetForm }) => {
      let errorMsgs = [];
      if (dirty) {
        setSaveDisabled(true);

        await submitFormSectionData(formSectionSubmitObject(values, activeGroupId), userUid).then(
          errors => {
            if (!errors.length) enqueueSnackbar('Submitted.', { variant: 'success' });
            else errorMsgs = errors;
          }
        );
      }
      if (!errorMsgs?.length) {
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
    setCurrentSectionParams(questions, activeGroup, searchParams, questionsGroups);
  }, [questions?.length, searchParams.get('section')]);

  useEffect(() => {
    setCurrentSectionQuestions(questions, activeGroup, questionsGroups, groupedQuestions);
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
