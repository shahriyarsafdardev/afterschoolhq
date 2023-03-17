import React, { useEffect } from 'react';
import TagManager from 'react-gtm-module';
import { useRoutes } from 'react-router-dom';
import { SnackbarProvider } from 'notistack';
import CssBaseline from '@material-ui/core/CssBaseline';
import { ThemeProvider } from '@material-ui/core/styles';
import { Provider, ErrorBoundary } from '@rollbar/react';

import GoogleTagManagerConfig from 'configs/googleTagManagerConfigs';
import UpdateProfile from 'pages/UpdateProfile/UpdateProfile';
import loadZendeskWidget from 'utils/loadZendeskWidget';
import RollbarConfig from 'configs/rollbarConfigs';
import loadSmartlook from 'utils/loadSmartlook';
import Home from './pages/registration/Home';
import Wrapper from 'pages/Wrapper/Wrapper';
import theme from './theme/theme';

function App() {
  const Router = () =>
    useRoutes([
      {
        element: <Wrapper />,
        path: '/',
        children: [
          {
            path: 'reactapp',
            element: <Home />
          }
        ]
      },
      {
        path: 'reactapp/updateprofile/:id',
        element: <UpdateProfile />
      }
    ]);

  useEffect(() => {
    TagManager.initialize(GoogleTagManagerConfig);
    loadZendeskWidget();
    loadSmartlook().then(smartlook => {
      // You can now use the smartlook object here
      smartlook;
    });
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
