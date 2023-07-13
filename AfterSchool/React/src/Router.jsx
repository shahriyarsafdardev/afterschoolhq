import React, { Suspense, lazy } from 'react';
import { useRoutes } from 'react-router-dom';

import { Loader } from 'components';
import utils from 'utils';

// eslint-disable-next-line react/display-name
const Loadable = Component => props => {
  return (
    <Suspense fallback={<Loader />}>
      <Component {...props} />
    </Suspense>
  );
};

function Router() {
  return useRoutes([
    {
      path: '/',
      element: <Wrapper />,
      children: [
        {
          path: 'reactapp/registration/:accountName/:activityName/:regUid',
          element: <Registration />
        },
        {
          path: 'reactapp/registration/new-child/:accountName/:activityName/:regUid',
          element: <Registration newChild={true} />
        },
        {
          path: 'reactapp/reg/update/:regUid',
          element: <UpdateRegistration />
        },
        {
          path: 'reactapp/registration/thankyou/:registrationId',
          element: <ThankYouSection />
        },
        {
          path: 'reactapp/volunteer-registration/thankyou/:registrationId',
          element: <ThankYouSection volunteerRegistration={true} />
        },
        {
          path: 'reactapp/volunteer-registration/:accountName/:activityName/:regUid',
          element: <VolunteerRegistration />
        },
        {
          path: 'reactapp/registration/payment/:registrationId',
          element: <AlreadyPaidSection />
        },
        {
          path: 'reactapp/reg/setup/:registrationUid',
          element: <PostRegistrationSetup />
        }
      ]
    },
    {
      path: '/reactapp/updateprofile/:id',
      element: <UpdateProfile />
    },
    {
      path: 'reactapp/updatevolunteerprofile/:id',
      element: <UpdateVolunteerProfile />
    },
    {
      path: '/Org/form/edit/:formId',
      element: <FormBuilderSection />
    },
    {
      path: '*',
      element: <RedirectToExternal />
    },
    {
      path: 'reactapp/survey/:formUid/thankyou',
      element: <SurveyThankYouSection />
    },
    {
      path: 'reactapp/sr/:srUid/thankyou',
      element: <PinSurveyThankYouSection />
    },
    {
      path: 'reactapp/survey/:formUid',
      element: <Survey />
    },
    {
      path: 'reactapp/sr/:srUid',
      element: <SurveyPinModal />
    },
    {
      path: 'reactapp/pin-setup/:registrationUid',
      element: <PinSetup />
    },
    {
      path: 'reactapp/check-in-out/:accountName',
      element: <CheckInOut />
    }
  ]);
}
const RedirectToExternal = () => {
  const { pathWithOrigin, redirect } = utils;

  redirect(pathWithOrigin('/User/home'));
  return null;
};
const Survey = Loadable(lazy(() => import('pages/Survey')));
const Wrapper = Loadable(lazy(() => import('pages/Wrapper/Wrapper')));
const SurveyPinModal = Loadable(lazy(() => import('pages/Survey/PinSurvey')));
const PostRegistrationSetup = Loadable(lazy(() => import('pages/PostRegistrationSetup')));
const UpdateRegistration = Loadable(lazy(() => import('pages/UpdateRegistration')));
const Registration = Loadable(lazy(() => import('pages/Registration/Registration')));
const FormBuilderSection = Loadable(lazy(() => import('sections/FormBuilderSection')));
const UpdateProfile = Loadable(lazy(() => import('pages/UpdateProfile/UpdateProfile')));
const SurveyThankYouSection = Loadable(lazy(() => import('sections/SurveyThankYouSection')));
const ThankYouSection = Loadable(lazy(() => import('sections/ThankYouSection/ThankYouSection')));
const PinSurveyThankYouSection = Loadable(lazy(() => import('sections/PinSurveyThankYouSection')));
const PinSetup = Loadable(lazy(() => import('pages/CheckInCheckOut/PinSetup')));
const CheckInOut = Loadable(lazy(() => import('pages/CheckInCheckOut/CheckInOut')));
const AlreadyPaidSection = Loadable(
  lazy(() => import('sections/AlreadyPaidSection/AlreadyPaidSection'))
);
const UpdateVolunteerProfile = Loadable(
  lazy(() => import('pages/UpdateVolunteerProfile/UpdateVolunteerProfile'))
);
const VolunteerRegistration = Loadable(
  lazy(() => import('pages/VolunteerRegistration/VolunteerRegistration'))
);

export default Router;
