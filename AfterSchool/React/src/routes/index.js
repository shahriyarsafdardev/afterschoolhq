export default {
  RegisterAPI: 'ReactApi/React/Register',
  currentUserAPI: 'ReactApi/React/GetCurrentUser',
  submitFormDataAPI: 'ReactApi/React/AddUpdateAnswers',
  SchoolAPI: 'ReactApi/React/CreateOrUpdateFromGoogle',
  updateProfileFormDataAPI: 'ReactApi/React/UpdateProfile',
  ThankYouAPI: id => `ReactApi/React/ThankYou/${id}`,
  getProfileFormDataAPI: id => `ReactApi/React/GetRegistrantForm/${id}`,
  getFormDataAPI: (accountName, activityName, registrantUid) =>
    `ReactApi/React/GetForm/?accountName=${accountName}&activityName=${activityName}&registrantUid=${registrantUid}`,
  getNewChildFormDataAPI: (accountName, activityName, registrantUid) =>
    `ReactApi/React/GetNewChildForm/?accountName=${accountName}&activityName=${activityName}&registrantUid=${registrantUid}`,

  paymentScreenPath: uId => `/account/payment/${uId}`,
  thankyouScreenPath: uId => `/account/thankyou/${uId}`,

  familyUrl: '/User/family',
  accountUrl: `${window.location.origin}/User/home`,
  registerForNewChildUrl: `${window.location.origin}/account/addstudent`,
  paymentUrl: registrationId => `/reactapp/registration/payment/${registrationId}`,
  thankyouUrl: registrationId => `/reactapp/registration/thankyou/${registrationId}`,
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
