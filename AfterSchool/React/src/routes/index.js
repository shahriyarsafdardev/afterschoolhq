export default {
  RegisterAPI: 'Api/Registration/Register',
  currentUserAPI: 'Api/Registration/GetCurrentUser',
  submitFormDataAPI: 'Api/Registration/AddUpdateAnswers',
  SchoolAPI: 'Api/Registration/CreateOrUpdateFromGoogle',
  updateProfileFormDataAPI: 'Api/Registration/UpdateProfile',
  ThankYouAPI: id => `Api/Registration/ThankYou/${id}`,
  getProfileFormDataAPI: id => `Api/Registration/GetRegistrantForm/${id}`,
  getFormDataAPI: (accountName, activityName, registrantUid) =>
    `Api/Registration/GetForm/?accountName=${accountName}&activityName=${activityName}&registrantUid=${registrantUid}`,
  getNewChildFormDataAPI: (accountName, activityName, registrantUid) =>
    `Api/Registration/GetNewChildForm/?accountName=${accountName}&activityName=${activityName}&registrantUid=${registrantUid}`,

  VolunteerRegisterAPI: 'Api/VolunteerRegistration/Register',
  submitVolunteerFormDataAPI: 'Api/VolunteerRegistration/AddUpdateAnswers',
  updateVolunteerProfileFormDataAPI: 'Api/VolunteerRegistration/UpdateProfile',
  VolunteerRegistrationThankYouAPI: id => `Api/VolunteerRegistration/ThankYou/${id}`,
  getVolunteerProfileFormDataAPI: id => `Api/VolunteerRegistration/GetRegistrantForm/${id}`,
  volunteerThankYouUrl: registrationId =>
    `/reactapp/volunteer-registration/thankyou/${registrationId}`,
  getVolunteerFormDataAPI: (accountName, activityName, regUid) =>
    `Api/VolunteerRegistration/GetForm?accountName=${accountName}&activityName=${activityName}&registrantUid=${regUid}`,

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
