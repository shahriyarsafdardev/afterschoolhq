import { useDispatch } from 'react-redux';
import { createSlice } from '@reduxjs/toolkit';

import { responseMapper } from './helper';
import SurveyRegistrationRepository from 'repository/SurveyRegistrationRepository';

const SurveyRegistrationSlice = createSlice({
  name: 'surveyRegistrationSlice',
  initialState: {
    msgs: [],
    questions: [],
    loading: true,
    questionsGroups: [],
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
    setFormData: (state, { payload: { FormDto } }) => {
      const { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer } = responseMapper(
        FormDto
      );

      state.questions = formQuestionUpdated;
      state.questionsGroups = formQuestionGroupUpdated;
      state.answeredQuestions = formQuestionAnswer;
    },
    setMsgs: (state, { payload }) => {
      state.msgs = payload;
    }
  }
});

export const useSurveyRegistrationActions = () => {
  const { setMsgs, setFormData, formDataLoaded, loadingFormData } = SurveyRegistrationSlice.actions;
  const dispatch = useDispatch();

  const loadFormData = async formId => {
    dispatch(loadingFormData());
    await SurveyRegistrationRepository.getSurveyFormData(formId).then(({ obj, msgs }) => {
      if (obj) dispatch(setFormData(obj));
      dispatch(setMsgs(msgs));
      if (!msgs.length) dispatch(formDataLoaded());
    });
  };

  const submitFormSectionData = async ({ answers, answerQuery, formData }, formId) => {
    formData.append('formId', formId);
    formData.append('surveyResponseId', null);
    formData.append('answers', new URLSearchParams(answerQuery).toString());
    await SurveyRegistrationRepository.submitFormSection(formData).then(({ data: { msgs } }) =>
      dispatch(setMsgs(msgs))
    );
  };

  return {
    loadFormData,
    submitFormSectionData
  };
};

export default SurveyRegistrationSlice;
