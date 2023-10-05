import { useDispatch } from 'react-redux';
import { createSlice } from '@reduxjs/toolkit';

import utils from 'utils';
import apiRoutes from 'routes';
import { responseMapper } from './helper';
import ErrorMessagesSlice from './ErrorMessagesSlice';
import SurveyRepository from 'repository/SurveyRepository';

const SurveySlice = createSlice({
  name: 'surveySlice',
  initialState: {
    questions: [],
    loading: true,
    questionsGroups: [],
    surveyHeaderData: {},
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
    setFormData: (state, { payload: { FormDto, ...headerData } }) => {
      const { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer } = responseMapper(
        FormDto
      );

      state.questions = formQuestionUpdated;
      state.questionsGroups = formQuestionGroupUpdated;
      state.answeredQuestions = formQuestionAnswer;
      state.surveyHeaderData = {
        activityName: headerData.ActivityName,
        providerName: headerData.ProviderName,
        studentName: headerData.StudentName,
        surveyName: headerData.SurveyName
      };
    },
    setAnsweredQuestions: (state, { payload }) => {
      state.answeredQuestions = payload;
    }
  }
});

export const useSurveyActions = () => {
  const { setMsgs } = ErrorMessagesSlice.actions;
  const {
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
    return await SurveyRepository.getSurveyFormData(formUid, surveyResponseUid, pin).then(
      ({ obj, msgs }) => {
        if (obj?.SurveyResponseStatus === 'Completed') return redirectToThankYou(obj?.Pin, formUid);
        if (obj) dispatch(setFormData(obj));
        if (!msgs.length) dispatch(formDataLoaded());
        return msgs;
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
      ({ data: { obj, msgs } }) => {
        if (obj?.FormQuestionAnswers.length)
          dispatch(setAnsweredQuestions(obj?.FormQuestionAnswers));
        dispatch(setResponseId(obj?.SurveyResponseUid));
        return [obj?.SurveyResponseUid, msgs];
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
