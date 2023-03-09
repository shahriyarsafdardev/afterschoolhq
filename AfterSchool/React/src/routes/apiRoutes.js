export default {
  RegisterAPI: 'ReactApi/React/Register',
  getFormDataAPI: 'ReactApi/React/GetForm',
  currentUserAPI: 'ReactApi/React/GetCurrentUser',
  accountUrl: `${window.location.origin}/User/home`,
  paymentScreenPath: uId => `/account/payment/${uId}`,
  submitFormDataAPI: 'ReactApi/React/AddUpdateAnswers',
  SchoolAPI: 'ReactApi/React/CreateOrUpdateFromGoogle',
  thankyouScreenPath: uId => `/account/thankyou/${uId}`,
  getNewChildFormDataAPI: 'ReactApi/React/GetNewChildForm',
  facebookShareUrl: (activityUrl, textToShare = '') =>
    `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(
      activityUrl
    )}&quote=${encodeURIComponent(textToShare)}`,
  twitterShareUrl: (activityName, appName, activityUrl) =>
    `http://twitter.com/share?text=${encodeURIComponent(
      `Check out ${activityName} on ${appName}`
    )}&url=${encodeURIComponent(
      activityUrl
    )}&hashtags=Education,AfterschoolActivities,AfterSchoolHQ`
};
