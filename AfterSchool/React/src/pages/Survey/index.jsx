import { useSearchParams, useNavigate, useParams } from 'react-router-dom';
import React, { useEffect, useState, useRef } from 'react';
import { useSnackbar } from 'notistack';
import { useFormik } from 'formik';

import RegistrationService from 'sevices/RegistrationService';
import RegistrationForm from 'pages/shared/RegistrationForm';
import { useSurvey } from 'redux/hooks';
import routes from 'routes';
import utils from 'utils';

export default function Survey() {
  const navigate = useNavigate();
  const { formUid } = useParams();
  const { surveyThankyouUrl } = routes;
  const { scrollToTop } = utils;
  const { enqueueSnackbar } = useSnackbar();
  const {
    msgs,
    loading,
    questions,
    loadFormData,
    clearMessages,
    questionsGroups,
    submitFormSectionData,
    answeredQuestions = []
  } = useSurvey();

  const responseId = useRef('');
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
    answeredQuestions,
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
    responseId.current
      ? `section=${encodeURIComponent(questionsGroups[index].Name)}&surveyResponseUid=${
          responseId.current
        }`
      : `section=${encodeURIComponent(questionsGroups[index].Name)}`;

  const handleSection = action => {
    if (action === 'next') {
      setActiveGroupId(questionsGroups[activeGroup + 1].Id);
      setSearchParams(buildQueryParams(activeGroup + 1));
      setActiveGroup(activeGroup + 1);
    } else if (action === 'previous') {
      setActiveGroupId(questionsGroups[activeGroup - 1].Id);
      setSearchParams(buildQueryParams(activeGroup - 1));
      setActiveGroup(activeGroup - 1);
      clearMessages();
    }
  };

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  useEffect(() => {
    if (responseId.current && !searchParams.get('surveyResponseUid'))
      location.search = location.search + `&surveyResponseUid=${responseId.current}`;
  }, [searchParams.get('surveyResponseUid')]);

  const lastSection = activeGroup === totalGroups - 1;
  const firstSection = activeGroup === 0;
  const initialvalues = initialValues();

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: initialvalues,
    validate: values => validator(values),
    onSubmit: async values => {
      setSaveDisabled(true);
      if (dirty || lastSection || firstSection) {
        const submitObj = formSectionSubmitObject(values, activeGroupId);
        await submitFormSectionData(submitObj, formUid, responseId.current, lastSection).then(
          respId => {
            responseId.current = respId;
            if (!msgs.length) {
              enqueueSnackbar('Submitted.', { variant: 'success' });
              lastSection && navigate(surveyThankyouUrl(formUid));
            }
          }
        );
      }

      handleSection('next');
      setSaveDisabled(false);
    }
  });
  const { handleSubmit, dirty, values } = formik;
  const isEmpty = !Object.values(values).some(x => x !== '');
  const skipAble = sectionSkipAble && isEmpty && !dirty && !lastSection;

  useEffect(() => {
    loadFormData(formUid, searchParams.get('surveyResponseUid'));
  }, []);

  useEffect(() => {
    responseId.current = searchParams.get('surveyResponseUid');
  }, [searchParams.get('surveyResponseUid')]);

  useEffect(() => {
    if (questionsGroups.length) setActiveGroupId(questionsGroups[activeGroup].Id);
  }, [questionsGroups.length]);

  useEffect(() => {
    if (questions.length) {
      let Id = '';
      const paramsQuery = buildQueryParams();
      if (searchParams.get('section')) {
        const groupIndex = questionsGroups.findIndex(
          section => section.Name === decodeURI(searchParams.get('section'))
        );
        if (groupIndex < 0) return setSearchParams(paramsQuery);
        Id = questionsGroups[groupIndex].Id;
        setActiveGroupId(Id);
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
      isSurvey={true}
      showExit={false}
    ></RegistrationForm>
  );
}
