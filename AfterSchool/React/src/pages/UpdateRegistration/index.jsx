import { useSearchParams, useNavigate, useParams } from 'react-router-dom';
import React, { useEffect, useState } from 'react';
import { useSnackbar } from 'notistack';
import { useFormik } from 'formik';

import { useUpdateRegistration } from 'redux/hooks';
import RegistrationService from 'sevices/RegistrationService';
import RegistrationForm from 'pages/shared/RegistrationForm';
import apiRoutes from 'routes';
import utils from 'utils';

export default function UpdateRegistration() {
  const { scrollToTop } = utils;
  const navigate = useNavigate();
  const { enqueueSnackbar } = useSnackbar();
  const { thankyouUrl, volunteerThankYouUrl } = apiRoutes;
  const {
    school,
    loading,
    questions,
    loadFormData,
    questionsGroups,
    answeredQuestions,
    setSchoolFieldValue,
    submitFormSectionData,
    clearMessages,
    activity: {
      IsFree,
      RegistrantType,
      IsAgeRestricted,
      MaxAge,
      MinAge,
      IsGradeRestricted,
      MaxGrade,
      MinGrade,
      IsGenderRestricted,
      EnforcedGender
    }
  } = useUpdateRegistration();

  const { regUid } = useParams();
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

  const handleRedirection = () => {
    const url = RegistrantType === 'Volunteer' ? volunteerThankYouUrl(regUid) : thankyouUrl(regUid);
    navigate(`${url}?update=1`);
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
    // validate: values => validator(values, genderRestrictions, ageRestrictions, gradeRestrictions),
    onSubmit: async (values, { resetForm }) => {
      setSaveDisabled(true);
      let schoolLocationID = null;
      let errors = [];
      if (!isEmpty || dirty || secondLastSection) {
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
          regUid
        ).then(errorMsgs => {
          if (!errorMsgs.length) enqueueSnackbar('Submitted.', { variant: 'success' });
          else errors = errorMsgs;
        });
      }
      if (!lastSection && !errors?.length) {
        handleSection('next');
        resetForm();
      } else {
        lastSection && !errors?.length && handleRedirection();
        setSaveDisabled(false);
      }
    }
  });
  const { handleSubmit, dirty, values } = formik;
  const isEmpty = !Object.values(values).some(x => x !== '');
  const skipAble = sectionSkipAble && isEmpty && !dirty;

  useEffect(() => {
    loadFormData(regUid);
  }, [regUid]);

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

  const updatedSaveButton = () => 'E-Sign';

  return (
    <RegistrationForm
      loading={loading}
      totalGroups={totalGroups}
      activeGroup={activeGroup}
      saveDisabled={saveDisabled}
      currentQuestions={currentQuestions}
      Name={Name}
      saveButtonText={lastSection ? updatedSaveButton : saveButtonText}
      handleSection={handleSection}
      lastSection={lastSection}
      formik={formik}
      handleSubmit={handleSubmit}
      skipAble={skipAble}
      registrantType={RegistrantType}
      isFree={IsFree}
      firstSection={activeGroup === 0}
      isUpdatingRegistration={true}
      showExit={false}
    ></RegistrationForm>
  );
}
