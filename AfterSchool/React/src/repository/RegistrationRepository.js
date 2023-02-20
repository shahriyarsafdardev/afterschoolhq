import axios from './axios/axios';

export default {
  getFormData: () => axios.get('http://localhost:54849/ReactApi/React/GetForm').then(res => res),
  submitFormSection: params =>
    axios.post('http://localhost:54849/reactapi/react/AddUpdateAnswers', params).then(res => res)
};
