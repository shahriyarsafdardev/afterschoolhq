import axios from './axios/axios';
import ApiRoutes from 'routes';

export default {
  submitFormSection: params =>
    axios.post(ApiRoutes.submitSurveyFormDataAPI, params).then(res => res),
  getSurveyFormData: (formUid, surveyResponseUid) =>
    axios.get(ApiRoutes.getSurveyFormDataAPI(formUid, surveyResponseUid)).then(res => res)
};
