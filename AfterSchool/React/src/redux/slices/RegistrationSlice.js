import { useDispatch } from 'react-redux';
import { createSlice } from '@reduxjs/toolkit';

import utils from 'utils';
import HeaderSlice from './HeaderSlice';
import schoolField from 'constants/schoolField';
import ErrorMessagesSlice from './ErrorMessagesSlice';
import ConfirmRegistrationSection from 'constants/eSignatureSection';
import RegistrationRepository from 'repository/RegistrationRepository';
import { responseMapper, handleRedirection, setStatuses } from './helper';

const RegistrationSlice = createSlice({
  name: 'registrationSlice',
  initialState: {
    school: {},
    activity: {},
    questions: [],
    loading: true,
    noSchool: false,
    questionsGroups: [],
    registrationId: null,
    answeredQuestions: []
  },
  reducers: {
    loadingFormData: state => {
      state.loading = true;
    },
    formDataLoaded: state => {
      state.loading = false;
    },
    formDataLoadingFailed: state => {
      state.loading = false;
    },
    setFormData: (state, { payload: { FormDto, activityDto } }) => {
      const {
        CanAccess,
        RegistrantType,
        SchoolLocation,
        IsAgeRestricted,
        IsGradeRestricted,
        IsGenderRestricted,
        RegistrationIsCompleted
      } = activityDto;

      const isProhibited = RegistrationIsCompleted || !CanAccess;
      if (isProhibited) return handleRedirection(activityDto);

      const { questions, questionsGroups } = ConfirmRegistrationSection();
      const { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer } = responseMapper(
        FormDto,
        RegistrantType,
        IsGradeRestricted,
        IsAgeRestricted,
        IsGenderRestricted
      );

      state.questions = [...questions, ...formQuestionUpdated, schoolField(RegistrantType)];
      state.questionsGroups = setStatuses([...formQuestionGroupUpdated, ...questionsGroups]);
      state.school = SchoolLocation || {};
      state.answeredQuestions = formQuestionAnswer;
      state.activity = activityDto;
    },
    submitFormSection: (state, { payload }) => {
      state.answeredQuestions = payload;
    },
    setRegistrationId: (state, { payload }) => {
      state.registrationId = payload;
    },
    setSchool: (state, { payload: { school, noSchool } }) => {
      state.school = school;
      state.noSchool = noSchool;
    },
    setStatusAsCompleted: (state, { payload }) => {
      const updatedGroups = state.questionsGroups;
      updatedGroups[payload] = { ...updatedGroups[payload], Status: 'completed' };
      state.questionsGroups = updatedGroups;
    }
  }
});

export const useRegistrationActions = () => {
  const {
    setSchool,
    setFormData,
    formDataLoaded,
    loadingFormData,
    submitFormSection,
    setRegistrationId,
    setStatusAsCompleted
  } = RegistrationSlice.actions;
  const { setMsgs } = ErrorMessagesSlice.actions;
  const { setHeaderData, loadingCompleted: headerLoadingCompleted } = HeaderSlice.actions;
  const dispatch = useDispatch();

  const loaded = () => dispatch(formDataLoaded());

  const loadFormData = async (newChild = false, accountName, activityName, registrantUid) => {
    dispatch(loadingFormData());
    await RegistrationRepository.getFormData(
      newChild,
      accountName,
      activityName,
      registrantUid
    ).then(({ obj, msgs }) => {
      if (obj) dispatch(setFormData(obj));
      if (!msgs.length && !obj?.activityDto?.RegistrationIsCompleted) {
        const { HeaderImageURL, Name, Organization } = obj?.activityDto;
        dispatch(setHeaderData({ HeaderImageURL, Name, Organization }));
        dispatch(headerLoadingCompleted());
        dispatch(formDataLoaded());
      }
    });
  };

  const submitFormSectionData = async (
    { answers, answerQuery, formData },
    lastSection,
    schoolLocationID,
    newChild,
    accountName,
    activityName,
    regUid
  ) => {
    const { answerQuery: answersQuery, groupId } = answerQuery;

    formData.append('groupId', groupId);
    formData.append('newChild', newChild);
    formData.append('registrantUid', regUid);
    formData.append('accountName', accountName);
    formData.append('activityName', activityName);
    formData.append('schoolLocationID', schoolLocationID);
    formData.append('answers', new URLSearchParams(answersQuery).toString());
    dispatch(submitFormSection(answers));

    if (lastSection) {
      const eSignature = answers.find(answer => answer.QuestionText === 'E-Signature').Text;
      formData.append('eSignature', eSignature);
      dispatch(loadingFormData());

      return RegistrationRepository.register(formData).then(({ data: { obj, msgs } }) => {
        dispatch(setRegistrationId(obj));
        return msgs;
      });
    } else
      return RegistrationRepository.submitFormSection(formData).then(({ data: { msgs } }) => msgs);
  };

  const setSchoolFieldValue = ({ school, noSchool }, submit) => {
    if (!submit) return dispatch(setSchool({ school, noSchool }));

    return RegistrationRepository.submitSchoolData(school).then(({ data: { obj, msgs } }) => {
      dispatch(setSchool({ school: { ...school, id: obj }, noSchool }));
      return { obj, msgs };
    });
  };

  const clearMessages = () => {
    dispatch(setMsgs([]));
  };

  const setStatusCompleted = groupId => {
    dispatch(setStatusAsCompleted(groupId));
  };

  const exit = async (uid, activityUrl) => {
    await RegistrationRepository.exit(uid).then(({ msgs }) => {
      const { redirect } = utils;
      if (!msgs.length) redirect(activityUrl);
    });
  };

  return {
    submitFormSectionData,
    setSchoolFieldValue,
    setStatusCompleted,
    clearMessages,
    loadFormData,
    loaded,
    exit
  };
};

export default RegistrationSlice;
