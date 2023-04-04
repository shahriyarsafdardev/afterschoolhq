export default {
  RegisterAPI: 'ReactApi/React/Register',
  getFormDataAPI: (accountName, activityName, registrantUid) =>
    `ReactApi/React/GetForm/?accountName=${accountName}&activityName=${activityName}&registrantUid=${registrantUid}`,
  currentUserAPI: 'ReactApi/React/GetCurrentUser',
  ThankYouAPI: id => `ReactApi/React/ThankYou/${id}`,
  submitFormDataAPI: 'ReactApi/React/AddUpdateAnswers',
  SchoolAPI: 'ReactApi/React/CreateOrUpdateFromGoogle',
  updateProfileFormDataAPI: 'ReactApi/React/UpdateProfile',
  getNewChildFormDataAPI: (accountName, activityName, registrantUid) =>
    `ReactApi/React/GetNewChildForm/?accountName=${accountName}&activityName=${activityName}&registrantUid=${registrantUid}`,
  getProfileFormDataAPI: id => `ReactApi/React/GetRegistrantForm/${id}`,

  paymentScreenPath: uId => `/account/payment/${uId}`,
  thankyouScreenPath: uId => `/account/thankyou/${uId}`,

  familyUrl: '/User/family',
  accountUrl: `${window.location.origin}/User/home`,
  registerForNewChildUrl: `${window.location.origin}/account/addstudent`,
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
