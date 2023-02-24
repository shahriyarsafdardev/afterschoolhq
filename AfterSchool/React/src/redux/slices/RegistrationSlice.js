import { createSlice } from '@reduxjs/toolkit';
import { useDispatch } from 'react-redux';
import RegistrationRepository from 'repository/RegistrationRepository';
import ConfirmRegistrationSection from 'constants/eSignatureSection';
import schoolField from 'constants/schoolField';

const RegistrationSlice = createSlice({
  name: 'registrationSlice',
  initialState: {
    questions: [],
    questionsGroups: [],
    answeredQuestions: [],
    activity: {},
    loading: true,
    registrationId: null
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
    setFormData: (state, { payload }) => {
      const eSignatureSection = ConfirmRegistrationSection();
      state.questions = [
        ...payload.FormDto.formQuestion,
        ...eSignatureSection.questions,
        schoolField
      ];
      state.questionsGroups = [
        ...payload.FormDto.formQuestionGroup,
        ...eSignatureSection.questionsGroups
      ];
      state.answeredQuestions = payload.FormDto.formQuestionAnswer;
      state.activity = payload.activityDto;
    },
    submitFormSection: (state, { payload }) => {
      state.answeredQuestions = payload;
    },
    setRegistrationId: (state, { payload }) => {
      state.registrationId = payload;
    }
  }
});

export const useRegistrationActions = () => {
  const { actions } = RegistrationSlice;
  const dispatch = useDispatch();

  const loadFormData = async () => {
    dispatch(actions.loadingFormData());
    const formData = await RegistrationRepository.getFormData();
    dispatch(actions.setFormData(formData.obj));
    dispatch(actions.formDataLoaded());
  };

  const submitFormSection = async (formSectionSubmitObject, lastSection) => {
    const { answers, answerQuery } = formSectionSubmitObject;
    const { answerQuery: answersQuery, groupId } = answerQuery;
    const params = { answers: new URLSearchParams(answersQuery).toString(), groupId };

    if (lastSection) {
      const eSignature = answers.find(answer => answer.QuestionText === 'E-Signature').Text;
      dispatch(actions.loadingFormData());
      RegistrationRepository.register({ ...params, eSignature }).then(res => {
        dispatch(actions.setRegistrationId(res.data.obj));
        dispatch(actions.formDataLoaded());
      });
    } else await RegistrationRepository.submitFormSection(params);

    dispatch(actions.submitFormSection(answers));
  };

  return { loadFormData, submitFormSection };
};

export default RegistrationSlice;
