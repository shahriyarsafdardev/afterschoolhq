import { useDispatch } from 'react-redux';
import { createSlice } from '@reduxjs/toolkit';

import utils from 'utils';
import apiRoutes from 'routes';
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

  const loadFormData = async (formUid, surveyResponseUid) => {
    dispatch(setResponseId(surveyResponseUid));
    dispatch(loadingFormData());
    await SurveyRegistrationRepository.getSurveyFormData(formUid, surveyResponseUid).then(
      ({ obj, msgs }) => {
        if (obj.SurveyResponseStatus === 'Completed') {
          return utils.navigateTo(apiRoutes.surveyThankyouUrl(formUid));
        }
        if (obj) dispatch(setFormData(obj));
        dispatch(setMsgs(msgs));
        if (!msgs.length) dispatch(formDataLoaded());
      }
    );
  };

  const submitFormSectionData = async (
    { answers, answerQuery: { answerQuery }, formData },
    formUid,
    surveyResponseUid,
    isLastSection
  ) => {
    formData.append('formUid', formUid);
    formData.append('isLastSection', isLastSection);
    formData.append('surveyResponseUid', surveyResponseUid);
    formData.append('answers', new URLSearchParams(answerQuery).toString());
    return await SurveyRegistrationRepository.submitFormSection(formData).then(
      ({ data: { msgs, obj } }) => {
        dispatch(setResponseId(obj?.SurveyResponseUid));
        dispatch(setMsgs(msgs));
        return obj?.SurveyResponseUid;
      }
    );
  };

  return {
    loadFormData,
    submitFormSectionData
  };
};

export default SurveyRegistrationSlice;
