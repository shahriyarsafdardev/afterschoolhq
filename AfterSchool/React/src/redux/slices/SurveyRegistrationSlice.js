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
    answeredQuestions: [],
    surveyResponseId: null
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
    setResponseId: (state, { payload }) => {
      if (state.surveyResponseId !== payload) state.surveyResponseId = payload;
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
  const {
    setMsgs,
    setFormData,
    setResponseId,
    formDataLoaded,
    loadingFormData
  } = SurveyRegistrationSlice.actions;
  const dispatch = useDispatch();

  const loadFormData = async (formId, surveyResponseId) => {
    dispatch(setResponseId(surveyResponseId));
    dispatch(loadingFormData());
    await SurveyRegistrationRepository.getSurveyFormData(formId, surveyResponseId).then(
      ({ obj, msgs }) => {
        if (obj) dispatch(setFormData(obj));
        dispatch(setMsgs(msgs));
        if (!msgs.length) dispatch(formDataLoaded());
      }
    );
  };

  const submitFormSectionData = async (
    { answers, answerQuery: { answerQuery }, formData },
    formId,
    surveyResponseId
  ) => {
    formData.append('formId', formId);
    formData.append('surveyResponseId', surveyResponseId);
    formData.append('answers', new URLSearchParams(answerQuery).toString());
    return await SurveyRegistrationRepository.submitFormSection(formData).then(
      ({ data: { msgs, obj } }) => {
        dispatch(setResponseId(obj?.SurveyResponseId));
        dispatch(setMsgs(msgs));
        return obj?.SurveyResponseId;
      }
    );
  };

  return {
    loadFormData,
    submitFormSectionData
  };
};

export default SurveyRegistrationSlice;
