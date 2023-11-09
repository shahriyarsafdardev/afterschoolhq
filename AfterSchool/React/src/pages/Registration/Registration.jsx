import { useSearchParams, useNavigate, useParams } from 'react-router-dom';
import React, { useEffect, useState } from 'react';
import { useSnackbar } from 'notistack';
import { useFormik } from 'formik';

import {
  checkEmptyValues,
  setSectionIndexToStorage,
  handleStatusAndRedirection
} from 'pages/shared/helper';
import RegistrationService from 'sevices/RegistrationService';
import RegistrationForm from 'pages/shared/RegistrationForm';
import SidebarSection from 'pages/shared/Sidebar';
import { useRegistration } from 'redux/hooks';
import { Box } from '@material-ui/core';
import apiRoutes from 'routes';
import utils from 'utils';

export default function Registration({ newChild = false }) {
  const navigate = useNavigate();
  const { redirect } = utils;
  const { paymentScreenPath, thankyouUrl, postRegistrationUrl } = apiRoutes;
  const { enqueueSnackbar } = useSnackbar();
  const {
    exit,
    school,
    loading,
    questions,
    loadFormData,
    registrationId,
    questionsGroups,
    answeredQuestions,
    setStatusCompleted,
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
    saveButtonText,
    groupedQuestions,
    handleSectionNavigation,
    formSectionSubmitObject,
    setCurrentSectionParams,
    setCurrentSectionQuestions
  } = registrationService;

  const handleSection = action =>
    handleSectionNavigation(action, activeGroup, clearMessages, questionsGroups);

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
      const groupId = secondLastSection ? -1 : activeGroupId;
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
          formSectionSubmitObject(values, groupId),
          lastSection,
          schoolLocationID,
          newChild ? 1 : 0,
          accountName,
          activityName,
          regUid
        ).then(msgs => {
          if (!msgs.length) {
            if (lastSection) localStorage.removeItem('lastCompletedSectionId');

            enqueueSnackbar('Submitted.', { variant: 'success' });
          } else errorMsgs.push(...msgs);
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
    loadFormData(newChild, accountName, activityName, regUid);
  }, [newChild, accountName, activityName]);

  useEffect(() => {
    if (questionsGroups.length)
      setActiveGroupId(questionsGroups[activeGroup].QuestionGroupDataTypeId);
  }, [questionsGroups.length]);

  useEffect(() => {
    setCurrentSectionParams(questions, activeGroup, searchParams, questionsGroups);
  }, [questions.length, searchParams.get('section')]);

  useEffect(() => {
    setCurrentSectionQuestions(questions, activeGroup, questionsGroups, groupedQuestions);
  }, [activeGroup, activeGroupId, questions]);

  const exitHandler = () => {
    exit(regUid, ActivityUrl);
  };

  return (
    <Box style={{ position: 'relative' }}>
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
        showProgressBar={false}
      ></RegistrationForm>
      {!loading && <SidebarSection questionsGroups={questionsGroups} />}
    </Box>
  );
}
