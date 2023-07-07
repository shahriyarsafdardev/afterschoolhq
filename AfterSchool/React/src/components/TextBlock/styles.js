import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
  fieldLabel: {
    width: '100%',
    maxWidth: 675,
    fontWeight: '400',
    display: 'flex',
    textAlign: 'justify',
    justifyContent: 'start',
    wordBreak: 'break-word',

    [theme.breakpoints.down('sm')]: {
      justifyContent: 'start',
      fontSize: '14px'
    }
  },
  outerWrapper: {
    maxWidth: '675px',
    margin: 'auto',
    marginBottom: '15px'
  },
  innerWrapper: {
    margin: 'auto'
  }
}));
