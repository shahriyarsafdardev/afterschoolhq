import { createSlice } from '@reduxjs/toolkit';
import { useDispatch } from 'react-redux';
import RegistrationRepository from '../../repository/RegistrationRepository';
import eSignatureSection from '../../constants/eSignatureSectionTest';

const RegistrationSlice = createSlice({
  name: 'registrationSlice',
  initialState: {
    questions: [],
    questionsGroups: [],
    answeredQuestions: [],
    activity: {},
    loading: true
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
      state.questions = [...payload.FormDto.formQuestion, ...eSignatureSection.questions];
      state.questionsGroups = [
        ...payload.FormDto.formQuestionGroup,
        ...eSignatureSection.questionsGroups
      ];
      state.answeredQuestions = payload.FormDto.formQuestionAnswer;
      state.activity = payload.activityDto;
    },
    submitFormSection: (state, { payload }) => {
      state.answeredQuestions = payload;
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

  const submitFormSection = async formSectionSubmitObject => {
    // const formData = await RegistrationRepository.getFormData();
    dispatch(actions.submitFormSection(formSectionSubmitObject));
  };

  return { loadFormData, submitFormSection };
};

export default RegistrationSlice;
