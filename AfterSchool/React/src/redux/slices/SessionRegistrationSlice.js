import { useDispatch } from 'react-redux';
import { createSlice } from '@reduxjs/toolkit';

import utils from 'utils';
import HeaderSlice from './HeaderSlice';
import schoolField from 'constants/schoolField';
import ErrorMessagesSlice from './ErrorMessagesSlice';
import { responseMapper, handleRedirection } from './helper';
import ConfirmRegistrationSection from 'constants/eSignatureSection';
import SessionRegistrationRepository from 'repository/SessionRegistrationRepository';

const SessionRegistrationSlice = createSlice({
  name: 'sessionRegistrationSlice',
  initialState: {
    school: {},
    session: {},
    checkOut: [],
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
    setFormData: (state, { payload: { FormDto, activityGroupDto, checkOut } }) => {
      const {
        CanAccess,
        RegistrantType,
        SchoolLocation,
        RegistrationIsCompleted
      } = activityGroupDto;

      let [isAgeRestricted, isGenderRestricted, isGradeRestricted] = [false, false, false];

      checkOut.forEach(({ IsAgeRestricted, IsGenderRestricted, IsGradeRestricted }) => {
        isAgeRestricted ||= IsAgeRestricted;
        isGenderRestricted ||= IsGenderRestricted;
        isGradeRestricted ||= IsGradeRestricted;
      });

      const isProhibited = RegistrationIsCompleted || !CanAccess;
      if (isProhibited) return handleRedirection(activityGroupDto);

      const { questions, questionsGroups } = ConfirmRegistrationSection();
      const { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer } = responseMapper(
        FormDto,
        RegistrantType,
        isGradeRestricted,
        isAgeRestricted,
        isGenderRestricted
      );

      state.checkOut = checkOut;
      state.questions = [...questions, ...formQuestionUpdated, schoolField(RegistrantType)];
      state.questionsGroups = [...formQuestionGroupUpdated, ...questionsGroups];
      state.school = SchoolLocation || {};
      state.answeredQuestions = formQuestionAnswer;
      state.session = activityGroupDto;
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
    }
  }
});

export const useSessionRegistrationActions = () => {
  const {
    setSchool,
    setFormData,
    formDataLoaded,
    loadingFormData,
    submitFormSection,
    setRegistrationId
  } = SessionRegistrationSlice.actions;
  const { setMsgs } = ErrorMessagesSlice.actions;
  const { setHeaderData, loadingCompleted: headerLoadingCompleted } = HeaderSlice.actions;
  const dispatch = useDispatch();

  const loaded = () => dispatch(formDataLoaded());

  const loadFormData = async (newChild = false, accountName, activityGroupName, registrantUid) => {
    dispatch(loadingFormData());
    await SessionRegistrationRepository.getFormData(
      newChild,
      accountName,
      activityGroupName,
      registrantUid
    ).then(({ obj, msgs }) => {
      if (obj) dispatch(setFormData(obj));
      if (!msgs.length && !obj?.activityGroupDto?.RegistrationIsCompleted) {
        const { HeaderImageURL, Name, Organization } = obj?.activityGroupDto;
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
    sessionName,
    regUid
  ) => {
    const { answerQuery: answersQuery, groupId } = answerQuery;

    formData.append('groupId', groupId);
    formData.append('newChild', newChild);
    formData.append('registrantUid', regUid);
    formData.append('accountName', accountName);
    formData.append('sessionName', sessionName);
    formData.append('schoolLocationID', schoolLocationID);
    formData.append('answers', new URLSearchParams(answersQuery).toString());
    dispatch(submitFormSection(answers));

    if (lastSection) {
      const eSignature = answers.find(answer => answer.QuestionText === 'E-Signature').Text;
      formData.append('eSignature', eSignature);
      dispatch(loadingFormData());

      return SessionRegistrationRepository.register(formData).then(({ data: { obj, msgs } }) => {
        dispatch(setRegistrationId(obj));
        return msgs;
      });
    } else
      return SessionRegistrationRepository.submitFormSection(formData).then(
        ({ data: { msgs } }) => msgs
      );
  };

  const setSchoolFieldValue = ({ school, noSchool }, submit) => {
    if (!submit) return dispatch(setSchool({ school, noSchool }));

    return SessionRegistrationRepository.submitSchoolData(school).then(
      ({ data: { obj, msgs } }) => {
        dispatch(setSchool({ school: { ...school, id: obj }, noSchool }));
        return { obj, msgs };
      }
    );
  };

  const clearMessages = () => {
    dispatch(setMsgs([]));
  };

  const exit = async (uid, sessionUrl) => {
    await SessionRegistrationRepository.exit(uid).then(({ msgs }) => {
      const { redirect } = utils;
      if (!msgs.length) redirect(sessionUrl);
    });
  };

  return { loadFormData, submitFormSectionData, setSchoolFieldValue, clearMessages, loaded, exit };
};

export default SessionRegistrationSlice;
