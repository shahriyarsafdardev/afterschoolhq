import React from 'react';
import { useRoutes } from 'react-router-dom';
import { SnackbarProvider } from 'notistack';
import CssBaseline from '@material-ui/core/CssBaseline';
import { ThemeProvider } from '@material-ui/core/styles';

import UpdateProfile from 'pages/UpdateProfile/UpdateProfile';
import Wrapper from 'pages/Wrapper/Wrapper';
import Home from './pages/registration/Home';
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

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <SnackbarProvider maxSnack={3} anchorOrigin={{ vertical: 'top', horizontal: 'right' }}>
        <Router />
      </SnackbarProvider>
    </ThemeProvider>
  );
}
export default App;
