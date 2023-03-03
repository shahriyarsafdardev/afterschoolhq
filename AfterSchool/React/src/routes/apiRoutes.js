export default {
  RegisterAPI: 'ReactApi/React/Register',
  getFormDataAPI: 'ReactApi/React/GetForm',
  currentUserAPI: 'ReactApi/React/GetCurrentUser',
  paymentScreenPath: uId => `/account/payment/${uId}`,
  submitFormDataAPI: 'ReactApi/React/AddUpdateAnswers',
  SchoolAPI: 'ReactApi/React/CreateOrUpdateFromGoogle',
  thankyouScreenPath: uId => `/account/thankyou/${uId}`,
  getNewChildFormDataAPI: 'ReactApi/React/GetNewChildForm'
};
