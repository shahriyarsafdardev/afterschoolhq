import { useDispatch } from 'react-redux';
import { responseMapper } from './helper';
import { createSlice } from '@reduxjs/toolkit';
import schoolField from 'constants/schoolField';
import ConfirmRegistrationSection from 'constants/eSignatureSection';
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
      //   const { questions, questionsGroups } = ConfirmRegistrationSection();
      const { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer } = responseMapper(
        FormDto
      );

      state.questions = [
        // ...questions,
        ...formQuestionUpdated
        // schoolField(activityDto.RegistrantType)
      ];
      state.questionsGroups = [
        ...formQuestionGroupUpdated
        // ...questionsGroups
      ];
      state.answeredQuestions = formQuestionAnswer;
      //   state.activity = activityDto;
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
    submitFormSection,
    setRegistrationId
  } = ProfileSlice.actions;
  const dispatch = useDispatch();

  const loaded = () => dispatch(formDataLoaded());

  const loadFormData = async (newChild = '0', UserUid) => {
    dispatch(loadingFormData());
    await ProfileRepository.getFormData(newChild, UserUid).then(({ obj, msgs }) => {
      if (obj) dispatch(setFormData(obj));
      dispatch(setMsgs(msgs));
      if (!msgs.length) dispatch(formDataLoaded());
    });
  };

  const submitFormSectionData = async (
    { answers, answerQuery },
    lastSection,
    schoolLocationID,
    newChild,
    UserUid
  ) => {
    const { answerQuery: answersQuery, groupId } = answerQuery;
    const params = {
      newChild,
      groupId,
      schoolLocationID,
      answers: new URLSearchParams(answersQuery).toString(),
      registrantId: UserUid
    };

    // if (lastSection) {
    //   const eSignature = answers.find(answer => answer.QuestionText === 'E-Signature').Text;
    //   dispatch(loadingFormData());
    //   ProfileRepository.register({ ...params, eSignature }).then(({ data: { obj, msgs } }) => {
    //     dispatch(setRegistrationId(obj));
    //     dispatch(setMsgs(msgs));
    //   });
    // } else
    await ProfileRepository.submitFormSection(params).then(({ data: { msgs } }) =>
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
