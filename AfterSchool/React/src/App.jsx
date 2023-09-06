import React, { useEffect } from 'react';
import CssBaseline from '@material-ui/core/CssBaseline';
import { ThemeProvider } from '@material-ui/core/styles';
import { Provider, ErrorBoundary } from '@rollbar/react';
import loadZendeskWidget from 'utils/loadZendeskWidget';
import RollbarConfig from 'configs/rollbarConfigs';
import InitAnalytics from 'analytics';
import theme from './theme/theme';
import SnackbarProvider from './SnackbarProvider';
import Router from 'Router';
import utils from 'utils';
import store from './redux/store';
import { injectStore } from 'repository/axios/axios';

function App() {
  const { getEnv } = utils;

  useEffect(() => {
    injectStore(store);
    if (getEnv() === 'Local') return;
    InitAnalytics();
    loadZendeskWidget();
  }, []);

  return (
    <Provider config={RollbarConfig}>
      <ErrorBoundary>
        <ThemeProvider theme={theme}>
          <CssBaseline />
          <SnackbarProvider>
            <Router />
          </SnackbarProvider>
        </ThemeProvider>
      </ErrorBoundary>
    </Provider>
  );
}
export default App;
