import { useSearchParams, useNavigate, useParams } from 'react-router-dom';
import React, { useEffect, useState } from 'react';
import { useSnackbar } from 'notistack';
import { useFormik } from 'formik';

import RegistrationService from 'sevices/RegistrationService';
import RegistrationForm from 'pages/shared/RegistrationForm';
import { useRegistration } from 'redux/hooks';
import apiRoutes from 'routes';
import utils from 'utils';

export default function Registration({ newChild = false }) {
  const navigate = useNavigate();
  const { scrollToTop, redirect } = utils;
  const { paymentScreenPath, thankyouUrl, postRegistrationUrl } = apiRoutes;
  const { enqueueSnackbar } = useSnackbar();
  const {
    exit,
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
    activity: {
      IsFree,
      IsPaid,
      AllowToPayLater,
      RegistrantType,
      IsAgeRestricted,
      MaxAge,
      MinAge,
      IsGradeRestricted,
      MaxGrade,
      MinGrade,
      IsGenderRestricted,
      EnforcedGender,
      ActivityUrl,
      IsRequireCheckInCheckOut
    }
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
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  useEffect(() => {
    sessionStorage.removeItem('isSessionCompleted');
    if (registrationId) handleRedirection();
  }, [registrationId]);

  const handleRedirection = () => {
    if (IsPaid && !IsFree) {
      redirect(paymentScreenPath(registrationId));
      return;
    }

    if (!IsPaid && IsFree && IsRequireCheckInCheckOut) {
      navigate(postRegistrationUrl(registrationId));
      return;
    }

    if (IsFree) {
      navigate(thankyouUrl(registrationId));
      return;
    }
  };

  const gradeRestrictions = {
    minGrade: MinGrade,
    maxGrade: MaxGrade,
    isGradeRestricted: IsGradeRestricted
  };
  const genderRestrictions = {
    enforcedGender: EnforcedGender,
    isGenderRestricted: IsGenderRestricted
  };
  const ageRestrictions = { isAgeRestricted: IsAgeRestricted, maxAge: MaxAge, minAge: MinAge };
  const secondLastSection = activeGroup === totalGroups - 2;
  const lastSection = activeGroup === totalGroups - 1;
  const initialvalues = initialValues();

  const formik = useFormik({
    enableReinitialize: true,
    initialValues: initialvalues,
    validate: values => validator(values, genderRestrictions, ageRestrictions, gradeRestrictions),
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
          activityName,
          regUid
        ).then(msgs => {
          errorMsgs.push(...msgs);
          if (!msgs.length) enqueueSnackbar('Submitted.', { variant: 'success' });
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
    exit(regUid, ActivityUrl);
  };

  return (
    <RegistrationForm
      allowToPayLater={AllowToPayLater}
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
