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
    }
  ]);
}
const RedirectToExternal = () => {
  const { pathWithOrigin, redirect } = utils;

  redirect(pathWithOrigin('/User/home'));
  return null;
};
const Wrapper = Loadable(lazy(() => import('pages/Wrapper/Wrapper')));
const Registration = Loadable(lazy(() => import('pages/Registration/Registration')));
const FormBuilderSection = Loadable(lazy(() => import('sections/FormBuilderSection')));
const UpdateProfile = Loadable(lazy(() => import('pages/UpdateProfile/UpdateProfile')));
const ThankYouSection = Loadable(lazy(() => import('sections/ThankYouSection/ThankYouSection')));
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
