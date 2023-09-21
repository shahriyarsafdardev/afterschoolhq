import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
  fieldLabel: {
    textAlign: 'left',
    display: '-webkit-inline-box',
    [theme.breakpoints.down('sm')]: {
      fontSize: '14px',
      display: 'inline-block'
    }
  },
  fieldLabelRequired: {
    color: 'red'
  },
  formControlClass: {
    display: 'block',
    maxWidth: 600,
    width: '100%',
    paddingTop: '8px',
    marginBottom: '15px',
    fontSize: '13px',
    lineHeight: '1.42857143',
    color: '#777777',
    backgroundColor: '#ffffff',
    backgroundImage: 'none',
    borderRadius: '0',
    '& .MuiOutlinedInput-root': {
      width: '100%',
      height: '40px'
    }
  },
  outerWrapper: {
    maxWidth: '600px',
    margin: 'auto'
  },
  fieldLabelWrapper: {
    display: 'flex'
  },
  textfieldWrapper: {
    display: 'flex',
    margin: 'auto'
  },
  helperText: {
    fontSize: '14.88px',
    margin: '0',
    '&.MuiFormHelperText-root.Mui-error': {
      color: '#DF1B41'
    }
  }
}));
