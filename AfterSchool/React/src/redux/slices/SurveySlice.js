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
  const redirectToThankYou = (pin, uid) => {
    if (Number(pin)) utils.navigateTo(apiRoutes.pinSurveyThankyouUrl(uid));
    else utils.navigateTo(apiRoutes.surveyThankyouUrl(uid));
  };

  const loadFormData = async (formUid, surveyResponseUid, pin) => {
    dispatch(setResponseId(surveyResponseUid));
    dispatch(loadingFormData());
    await SurveyRepository.getSurveyFormData(formUid, surveyResponseUid, pin).then(
      ({ obj, msgs }) => {
        dispatch(setMsgs(msgs));
        if (obj?.SurveyResponseStatus === 'Completed') return redirectToThankYou(obj?.Pin, formUid);
        if (obj) dispatch(setFormData(obj));
        if (!msgs.length) dispatch(formDataLoaded());
      }
    );
  };

  const submitFormSectionData = async (
    { answerQuery: { answerQuery, groupId }, formData },
    formUid,
    surveyResponseUid,
    isLastSection,
    srUid,
    pin
  ) => {
    if (pin) {
      formData.append('surveyRequestUid', srUid);
      formData.append('pin', pin);
    } else {
      formData.append('formUid', formUid);
      formData.append('surveyResponseUid', surveyResponseUid);
    }
    formData.append('groupId', groupId);
    formData.append('isLastSection', isLastSection);
    formData.append('answers', new URLSearchParams(answerQuery).toString());
    return await SurveyRepository.submitFormSection(formData, pin).then(
      ({ data: { msgs, obj } }) => {
        dispatch(setAnsweredQuestions(obj?.FormQuestionAnswers));
        dispatch(setResponseId(obj?.SurveyResponseUid));
        dispatch(setMsgs(msgs));
        return obj?.SurveyResponseUid;
      }
    );
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
