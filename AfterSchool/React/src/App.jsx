import React, { useEffect } from 'react';
import { SnackbarProvider } from 'notistack';
import CssBaseline from '@material-ui/core/CssBaseline';
import { ThemeProvider } from '@material-ui/core/styles';
import { Provider, ErrorBoundary } from '@rollbar/react';

import loadZendeskWidget from 'utils/loadZendeskWidget';
import RollbarConfig from 'configs/rollbarConfigs';
import InitAnalytics from 'analytics';
import theme from './theme/theme';
import Router from 'Router';
import utils from 'utils';

function App() {
  const { getEnv } = utils;

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
