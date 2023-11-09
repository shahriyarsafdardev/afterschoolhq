import { useFormik } from 'formik';
import { useSnackbar } from 'notistack';
import React, { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';

import { completeLoadingAndRedirect } from 'pages/shared/helper';
import RegistrationService from 'sevices/RegistrationService';
import RegistrationForm from 'pages/shared/RegistrationForm';
import { useUser, useProfile } from 'redux/hooks/index';
import apiRoutes from 'routes';

export default function UpdateProfile() {
  const { loadUser } = useUser();
  const { familyUrl } = apiRoutes;
  const { enqueueSnackbar } = useSnackbar();
  const {
    loaded,
    school,
    loading,
    questions,
    loadFormData,
    clearMessages,
    RegistrantType,
    questionsGroups,
    answeredQuestions,
    setSchoolFieldValue,
    submitFormSectionData
  } = useProfile();

  const totalGroups = questionsGroups?.length || 1;
  const [activeGroup, setActiveGroup] = useState(0);
  const [activeGroupId, setActiveGroupId] = useState(0);
  const [saveDisabled, setSaveDisabled] = useState(false);
  const [searchParams, setSearchParams] = useSearchParams();
  const [currentQuestions, setCurrentQuestions] = useState([]);
  const userUid = window.location.pathname.split('/')[3];

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

  const handleRedirection = () => completeLoadingAndRedirect(loaded, familyUrl);

  const lastSection = activeGroup === totalGroups - 1;
  const initialvalues = initialValues();

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: initialvalues,
    validate: values => validator(values),
    onSubmit: async (values, { resetForm }) => {
      const errorMsgs = [];
      if (dirty) {
        setSaveDisabled(true);

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
        } else {
          schoolLocationID = school?.ID;
        }

        await submitFormSectionData(
          formSectionSubmitObject(values, activeGroupId),
          userUid,
          schoolLocationID
        ).then(msgs => {
          if (!msgs.length) enqueueSnackbar('Submitted.', { variant: 'success' });
          else errorMsgs.push(...msgs);
        });
      }
      if (!errorMsgs.length) {
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
