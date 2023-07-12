import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
  wrapper: {
    backgroundColor: '#00517C',
    height: '100vh'
  },
  innerContainer: {
    height: '70%',
    width: '70%',
    margin: 'auto',
    border: `8px solid ${theme.palette.primary.main}`,
    [theme.breakpoints.down('sm')]: {
      height: '95%',
      width: '90%'
    }
  },
  content: {
    width: '80%',
    margin: 'auto',
    display: 'flex',
    justifyContent: 'center',
    height: '100%',
    [theme.breakpoints.down('sm')]: {
      width: '90%'
    }
  },
  phoneForm: {
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'center',
    gap: 16,
    margin: 'auto',
    width: '40%',
    [theme.breakpoints.down('sm')]: {
      width: '100%'
    }
  },
  pinSetupForm: {
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'center',
    gap: 16,
    margin: 'auto',
    width: '80%',
    [theme.breakpoints.down('sm')]: {
      width: '100%'
    }
  },
  buttonNext: {
    width: '150px',
    margin: '10px auto',
    [theme.breakpoints.down('sm')]: {
      width: '90%'
    }
  },
  headerText: {
    textAlign: 'center',
    fontWeight: 600
  },
  subHeaderText: {
    textAlign: 'center',
    fontWeight: 400
  },
  pinInputField: {
    margin: 'auto'
  }
}));
