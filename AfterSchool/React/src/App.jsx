import React, { useEffect } from 'react';
import { useRoutes } from 'react-router-dom';
import { SnackbarProvider } from 'notistack';
import CssBaseline from '@material-ui/core/CssBaseline';
import { ThemeProvider } from '@material-ui/core/styles';
import { Provider, ErrorBoundary } from '@rollbar/react';

import UpdateProfile from 'pages/UpdateProfile/UpdateProfile';
import loadZendeskWidget from 'utils/loadZendeskWidget';
import RollbarConfig from 'configs/rollbarConfigs';
import Home from './pages/registration/Home';
import Wrapper from 'pages/Wrapper/Wrapper';
import { ThankYouSection } from 'sections';
import InitAnalytics from 'analytics';
import theme from './theme/theme';
import utils from 'utils';

function App() {
  const { getEnv } = utils;
  const Router = () =>
    useRoutes([
      {
        element: <Wrapper />,
        path: '/',
        children: [
          {
            path: 'reactapp',
            element: <Home />
          },
          {
            path: 'reactapp/registration/thankyou/:registrationId',
            element: <ThankYouSection />
          }
        ]
      },
      {
        path: 'reactapp/updateprofile/:id',
        element: <UpdateProfile />
      }
    ]);

  useEffect(() => {
    if (getEnv() === 'Local') return;
    InitAnalytics();
    loadZendeskWidget();
  }, []);

  return (
    <Provider config={RollbarConfig}>
      <ErrorBoundary>
        <ThemeProvider theme={theme}>
          <CssBaseline />
          <SnackbarProvider maxSnack={3} anchorOrigin={{ vertical: 'top', horizontal: 'right' }}>
            <Router />
          </SnackbarProvider>
        </ThemeProvider>
      </ErrorBoundary>
    </Provider>
  );
}
export default App;
