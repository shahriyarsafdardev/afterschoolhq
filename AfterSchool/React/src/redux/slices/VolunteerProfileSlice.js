import { useDispatch } from 'react-redux';
import { responseMapper } from './helper';
import { createSlice } from '@reduxjs/toolkit';
import ProfileRepository from 'repository/ProfileRepository';

const VolunteerProfileSlice = createSlice({
  name: 'volunteerProfileSlice',
  initialState: {
    msgs: [],
    activity: {},
    questions: [],
    loading: true,
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
    setFormData: (state, { payload: { FormDto, registrantType } }) => {
      const { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer } = responseMapper(
        FormDto,
        registrantType
      );

      state.questions = formQuestionUpdated;
      state.questionsGroups = formQuestionGroupUpdated;
      state.answeredQuestions = formQuestionAnswer;
      state.RegistrantType = registrantType;
    },
    submitFormSection: (state, { payload }) => {
      state.answeredQuestions = payload;
    },
    setRegistrationId: (state, { payload }) => {
      state.registrationId = payload;
    },
    setMsgs: (state, { payload }) => {
      state.msgs = payload;
    }
  }
});

export const useVolunteerProfileActions = () => {
  const {
    setMsgs,
    setFormData,
    formDataLoaded,
    loadingFormData,
    submitFormSection
  } = VolunteerProfileSlice.actions;
  const dispatch = useDispatch();

  const loaded = () => dispatch(formDataLoaded());

  const loadFormData = async UserUid => {
    dispatch(loadingFormData());
    await ProfileRepository.getFormData(UserUid, true).then(({ obj, msgs }) => {
      if (obj) dispatch(setFormData(obj));
      dispatch(setMsgs(msgs));
      if (!msgs.length) dispatch(formDataLoaded());
    });
  };

  const submitFormSectionData = async ({ answers, answerQuery, formData }, UserUid) => {
    const { answerQuery: answersQuery, groupId } = answerQuery;

    formData.append('groupId', groupId);
    formData.append('registrantId', UserUid);
    formData.append('answers', new URLSearchParams(answersQuery).toString());

    await ProfileRepository.submitFormSection(formData, true).then(({ data: { msgs } }) =>
      dispatch(setMsgs(msgs))
    );

    dispatch(submitFormSection(answers));
  };

  const clearMessages = () => {
    dispatch(setMsgs([]));
  };

  return { loadFormData, submitFormSectionData, clearMessages, loaded };
};

export default VolunteerProfileSlice;
