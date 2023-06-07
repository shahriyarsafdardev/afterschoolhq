import { useDispatch } from 'react-redux';
import { createSlice } from '@reduxjs/toolkit';

import utils from 'utils';
import apiRoutes from 'routes';
import { responseMapper } from './helper';
import SurveyRepository from 'repository/SurveyRepository';

const SurveySlice = createSlice({
  name: 'surveySlice',
  initialState: {
    msgs: [],
    questions: [],
    loading: true,
    questionsGroups: [],
    answeredQuestions: [],
    surveyResponseUid: null
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
      if (state.surveyResponseUid !== payload) state.surveyResponseUid = payload;
    },
    setFormData: (state, { payload: { FormDto } }) => {
      const { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer } = responseMapper(
        FormDto
      );

      state.questions = formQuestionUpdated;
      state.questionsGroups = formQuestionGroupUpdated;
      state.answeredQuestions = formQuestionAnswer;
    },
    setAnsweredQuestions: (state, { payload }) => {
      state.answeredQuestions = payload;
    },
    setMsgs: (state, { payload }) => {
      state.msgs = payload;
    }
  }
});

export const useSurveyActions = () => {
  const {
    setMsgs,
    setFormData,
    setResponseId,
    formDataLoaded,
    loadingFormData,
    setAnsweredQuestions
  } = SurveySlice.actions;
  const dispatch = useDispatch();

  const loadFormData = async (formUid, surveyResponseUid) => {
    dispatch(setResponseId(surveyResponseUid));
    dispatch(loadingFormData());
    await SurveyRepository.getSurveyFormData(formUid, surveyResponseUid).then(({ obj, msgs }) => {
      dispatch(setMsgs(msgs));
      if (obj?.SurveyResponseStatus === 'Completed') {
        return utils.navigateTo(apiRoutes.surveyThankyouUrl(formUid));
      }
      if (obj) dispatch(setFormData(obj));
      if (!msgs.length) dispatch(formDataLoaded());
    });
  };

  const submitFormSectionData = async (
    { answerQuery: { answerQuery, groupId }, formData },
    formUid,
    surveyResponseUid,
    isLastSection
  ) => {
    formData.append('groupId', groupId);
    formData.append('formUid', formUid);
    formData.append('isLastSection', isLastSection);
    formData.append('surveyResponseUid', surveyResponseUid);
    formData.append('answers', new URLSearchParams(answerQuery).toString());
    return await SurveyRepository.submitFormSection(formData).then(({ data: { msgs, obj } }) => {
      dispatch(setAnsweredQuestions(obj?.FormQuestionAnswers));
      dispatch(setResponseId(obj?.SurveyResponseUid));
      dispatch(setMsgs(msgs));
      return obj?.SurveyResponseUid;
    });
  };

  const clearMessages = () => {
    dispatch(setMsgs([]));
  };

  return {
    loadFormData,
    clearMessages,
    submitFormSectionData
  };
};

export default SurveySlice;
