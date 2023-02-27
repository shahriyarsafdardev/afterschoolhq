import React from 'react';
import { useRoutes } from 'react-router-dom';
import { SnackbarProvider } from 'notistack';
import CssBaseline from '@material-ui/core/CssBaseline';
import { ThemeProvider } from '@material-ui/core/styles';

import Home from './pages/registration/Home';
import theme from './theme/theme';

function App() {
  const Router = () =>
    useRoutes([
      {
        path: '/',
        children: [
          {
            path: 'reactapp',
            element: <Home />
          }
        ]
      }
    ]);

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <SnackbarProvider maxSnack={3}>
        <Router />
      </SnackbarProvider>
    </ThemeProvider>
  );
}
export default App;
