import React from 'react';
import { useRoutes } from 'react-router-dom';
import { ThemeProvider } from '@material-ui/core/styles';
import CssBaseline from '@material-ui/core/CssBaseline';
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
      <Router />
    </ThemeProvider>
  );
}
export default App;
