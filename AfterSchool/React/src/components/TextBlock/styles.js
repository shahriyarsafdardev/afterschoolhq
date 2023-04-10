import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
  fieldLabel: {
    width: '100%',
    maxWidth: 540,
    display: 'flex',
    textAlign: 'justify',
    justifyContent: 'start',
    padding: '0px 12px',

    [theme.breakpoints.down('sm')]: {
      justifyContent: 'start',
      padding: '0px 12px',
      fontSize: '14px'
    }
  },
  textFieldGrid: {
    [theme.breakpoints.down('sm')]: {
      width: '100%'
    }
  },
  titleGrid: {
    [theme.breakpoints.down('sm')]: {
      width: '100%'
    }
  },
  textFieldBox: {
    display: 'flex',
    justifyContent: 'flex-end',
    alignItems: 'center',
    [theme.breakpoints.down('sm')]: {
      flexDirection: 'column',
      justifyContent: 'flex-start'
    }
  }
}));
