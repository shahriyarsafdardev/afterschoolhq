import axios from './axios/axios';
import ApiRoutes from 'routes';

export default {
  saveNewSectionData: params =>
    axios.post(ApiRoutes.saveNewSectionDataAPI, params).then(res => res),
  submitFormSection: params =>
    axios.post(ApiRoutes.submitFormBuilderDataAPI, params).then(res => res),
  getFormData: formId => axios.get(ApiRoutes.getFormBuilderDataAPI(formId)).then(res => res),
  getSectionData: (formId, sectionId) =>
    axios.get(ApiRoutes.getFormBuilderSectionDataAPI(formId, sectionId)).then(res => res)
};
