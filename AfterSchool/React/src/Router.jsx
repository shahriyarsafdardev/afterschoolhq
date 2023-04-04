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
          element: <Home />
        },
        {
          path: 'reactapp/registration/new-child/:accountName/:activityName/:regUid',
          element: <Home newChild={true} />
        },
        {
          path: 'reactapp/registration/thankyou/:registrationId',
          element: <ThankYouSection />
        }
      ]
    },
    {
      path: '/reactapp/updateprofile/:id',
      element: <UpdateProfile />
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
const Home = Loadable(lazy(() => import('pages/registration/Home')));
const Wrapper = Loadable(lazy(() => import('pages/Wrapper/Wrapper')));
const UpdateProfile = Loadable(lazy(() => import('pages/UpdateProfile/UpdateProfile')));
const ThankYouSection = Loadable(lazy(() => import('sections/ThankYouSection/ThankYouSection')));

export default Router;
