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

  VolunteerRegisterAPI: 'ReactApi/VolunteerRegistration/Register',
  submitVolunteerFormDataAPI: 'ReactApi/VolunteerRegistration/AddUpdateAnswers',
  updateVolunteerProfileFormDataAPI: 'ReactApi/VolunteerRegistration/UpdateProfile',
  VolunteerRegistrationThankYouAPI: id => `ReactApi/VolunteerRegistration/ThankYou/${id}`,
  getVolunteerProfileFormDataAPI: id => `ReactApi/VolunteerRegistration/GetRegistrantForm/${id}`,
  volunteerThankYouUrl: registrationId =>
    `/reactapp/volunteer-registration/thankyou/${registrationId}`,
  getVolunteerFormDataAPI: (accountName, activityName, regUid) =>
    `ReactApi/VolunteerRegistration/GetForm?accountName=${accountName}&activityName=${activityName}&registrantUid=${regUid}`,

  paymentScreenPath: uId => `/account/payment/${uId}`,
  thankyouScreenPath: uId => `/account/thankyou/${uId}`,

  familyUrl: '/User/family',
  accountUrl: `${window.location.origin}/User/home`,
  volunteerAccountUrl: `${window.location.origin}/Volunteer/home`,
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
