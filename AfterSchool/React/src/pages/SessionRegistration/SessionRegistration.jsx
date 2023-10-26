import { useFormik } from 'formik';
import { useSnackbar } from 'notistack';
import React, { useEffect, useState } from 'react';
import { useSearchParams, useNavigate, useParams } from 'react-router-dom';

import utils from 'utils';
import apiRoutes from 'routes';
import { useSessionRegistration } from 'redux/hooks';
import RegistrationForm from 'pages/shared/RegistrationForm';
import RegistrationService from 'sevices/RegistrationService';
import DrawerSidebar from 'components/DrawerSidebar/DrawerSidebar';

export default function SessionRegistration({ newChild = false }) {
  const navigate = useNavigate();
  const { scrollToTop } = utils;
  const { sessionThankYouUrl } = apiRoutes;
  const { enqueueSnackbar } = useSnackbar();
  const {
    exit,
    school,
    loading,
    checkOut,
    questions,
    loadFormData,
    registrationId,
    questionsGroups,
    answeredQuestions,
    setSchoolFieldValue,
    submitFormSectionData,
    clearMessages,
    session: { RegistrantType, ActivityUrl: activityGroupUrl }
  } = useSessionRegistration();

  const totalGroups = questionsGroups?.length || 1;
  const [activeGroup, setActiveGroup] = useState(0);
  const [activeGroupId, setActiveGroupId] = useState(0);
  const [saveDisabled, setSaveDisabled] = useState(false);
  const [searchParams, setSearchParams] = useSearchParams();
  const [currentQuestions, setCurrentQuestions] = useState([]);
  const { accountName, activityGroupName, regUid } = useParams();
  const { sectionSkipAble, Name } = questionsGroups[activeGroup] || {};
  const registrationService = new RegistrationService(
    questions,
    currentQuestions,
    answeredQuestions,
    false,
    true
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
    sessionStorage.removeItem('isSessionCompleted');
    if (registrationId) navigate(sessionThankYouUrl(registrationId));
  }, [registrationId]);

  const secondLastSection = activeGroup === totalGroups - 2;
  const lastSection = activeGroup === totalGroups - 1;
  const initialvalues = initialValues();

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: initialvalues,
    validate: values => validator(values, {}, {}, {}, checkOut),
    onSubmit: async (values, { resetForm }) => {
      const errorMsgs = [];
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
        } else {
          schoolLocationID = school?.ID;
        }

        await submitFormSectionData(
          formSectionSubmitObject(values, secondLastSection ? -1 : activeGroupId),
          lastSection,
          schoolLocationID,
          newChild ? 1 : 0,
          accountName,
          activityGroupName,
          regUid
        ).then(msgs => {
          if (!msgs.length) enqueueSnackbar('Submitted.', { variant: 'success' });
          else errorMsgs.push(...msgs);
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
    loadFormData(newChild, accountName, activityGroupName, regUid);
  }, [newChild, accountName, activityGroupName]);

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
          section => section.Name === decodeURI(searchParams.get('section'))
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

  const exitHandler = () => {
    exit(regUid, activityGroupUrl);
  };

  return (
    <>
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
        isFree={true}
        firstSection={activeGroup === 0}
        exitHandler={exitHandler}
        isSession={true}
      ></RegistrationForm>
      {!loading && <DrawerSidebar />}
    </>
  );
}
