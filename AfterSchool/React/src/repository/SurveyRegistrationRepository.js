import axios from './axios/axios';
import ApiRoutes from 'routes';

export default {
  submitFormSection: params =>
    axios.post(ApiRoutes.submitSurveyFormDataAPI, params).then(res => res),
  getSurveyFormData: formId => axios.get(ApiRoutes.getSurveyFormDataAPI(formId)).then(res => res)
};
