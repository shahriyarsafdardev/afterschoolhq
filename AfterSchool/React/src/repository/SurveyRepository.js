import axios from './axios/axios';
import ApiRoutes from 'routes';

export default {
  submitFormSection: (params, pin) =>
    axios
      .post(
        pin ? ApiRoutes.submitSurveyFormDataWithPinAPI : ApiRoutes.submitSurveyFormDataAPI,
        params
      )
      .then(res => res),
  getSurveyFormData: (formUid, surveyResponseUid, pin) =>
    axios
      .get(
        pin
          ? ApiRoutes.getSurveyFormDataWithPinAPI(formUid, pin)
          : ApiRoutes.getSurveyFormDataAPI(formUid, surveyResponseUid)
      )
      .then(res => res)
};
