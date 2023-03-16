import React, { useEffect } from 'react';
import { useRoutes } from 'react-router-dom';
import { SnackbarProvider } from 'notistack';
import CssBaseline from '@material-ui/core/CssBaseline';
import { ThemeProvider } from '@material-ui/core/styles';
import { Provider, ErrorBoundary } from '@rollbar/react';

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
    loadZendeskWidget(
      'https://assets.zendesk.com/embeddable_framework/main.js',
      'afterschoolhq.zendesk.com'
    );
    loadSmartlook('smartlookKey', document).then(smartlook => {
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
