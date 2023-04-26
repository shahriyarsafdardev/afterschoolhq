import { isEmpty } from 'lodash';
import { useDispatch } from 'react-redux';
import { responseMapper } from './helper';
import { createSlice } from '@reduxjs/toolkit';
import schoolField from 'constants/schoolField';
import ProfileRepository from 'repository/ProfileRepository';

const ProfileSlice = createSlice({
  name: 'profileSlice',
  initialState: {
    msgs: [],
    school: {},
    activity: {},
    questions: [],
    loading: true,
    noSchool: false,
    RegistrantType: '',
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
    setFormData: (state, { payload: { FormDto, registrantType, SchoolLocation } }) => {
      const { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer } = responseMapper(
        FormDto,
        registrantType
      );

      state.questions = [...formQuestionUpdated, schoolField(registrantType)];
      state.questionsGroups = [...formQuestionGroupUpdated];
      state.answeredQuestions = formQuestionAnswer;
      state.school = SchoolLocation || {};
      state.noSchool = isEmpty(SchoolLocation) ? true : false;
      state.RegistrantType = registrantType;
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
    setMsgs: (state, { payload }) => {
      state.msgs = payload;
    }
  }
});

export const useProfileActions = () => {
  const {
    setMsgs,
    setSchool,
    setFormData,
    formDataLoaded,
    loadingFormData,
    submitFormSection
  } = ProfileSlice.actions;
  const dispatch = useDispatch();

  const loaded = () => dispatch(formDataLoaded());

  const loadFormData = async UserUid => {
    dispatch(loadingFormData());
    await ProfileRepository.getFormData(UserUid).then(({ obj, msgs }) => {
      if (obj) dispatch(setFormData(obj));
      dispatch(setMsgs(msgs));
      if (!msgs.length) dispatch(formDataLoaded());
    });
  };

  const submitFormSectionData = async (
    { answers, answerQuery, formData },
    UserUid,
    schoolLocationID
  ) => {
    const { answerQuery: answersQuery, groupId } = answerQuery;

    formData.append('groupId', groupId);
    formData.append('answers', new URLSearchParams(answersQuery).toString());
    formData.append('registrantId', UserUid);
    formData.append('schoolLocationID', schoolLocationID);

    await ProfileRepository.submitFormSection(formData).then(({ data: { msgs } }) =>
      dispatch(setMsgs(msgs))
    );

    dispatch(submitFormSection(answers));
  };

  const setSchoolFieldValue = ({ school, noSchool }, submit) => {
    if (!submit) return dispatch(setSchool({ school, noSchool }));

    return ProfileRepository.submitSchoolData(school).then(({ data: { obj, msgs } }) => {
      dispatch(setMsgs(msgs));
      dispatch(setSchool({ school: { ...school, id: obj }, noSchool }));
      return { obj, msgs };
    });
  };

  const clearMessages = () => {
    dispatch(setMsgs([]));
  };

  return { loadFormData, submitFormSectionData, setSchoolFieldValue, clearMessages, loaded };
};

export default ProfileSlice;
