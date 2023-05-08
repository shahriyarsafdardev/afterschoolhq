import axios from './axios/axios';
import ApiRoutes from 'routes';

export default {
  submitFormSection: params =>
    axios.post(ApiRoutes.submitFormBuilderDataAPI, params).then(res => res),
  getFormData: formId => axios.get(ApiRoutes.getFormBuilderDataAPI(formId)).then(res => res),
  getSectionData: (formId, sectionId) =>
    axios.get(ApiRoutes.getFormBuilderSectionDataAPI(formId, sectionId)).then(res => res)
};
