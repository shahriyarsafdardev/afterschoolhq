import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
  fieldLabel: {
    textAlign: 'left',
    display: '-webkit-inline-box',
    [theme.breakpoints.down('sm')]: {
      display: 'inline-block',
      fontSize: '14px'
    }
  },
  formControlClass: {
    border: '1px solid #c4c4c4',
    marginTop: '10px',
    display: 'block',
    maxWidth: 600,
    width: '100%',
    paddingTop: '8px',
    fontSize: '13px',
    lineHeight: '1.42857143',
    color: '#777777',
    backgroundColor: '#ffffff',
    backgroundImage: 'none',
    borderRadius: '5px',
    '& .MuiOutlinedInput-root': {
      width: '100%',
      minHeight: '40px'
    }
  },
  flexCenter: {
    display: 'flex',
    paddingLeft: 14
  },
  outerWrapper: {
    maxWidth: '600px',
    margin: 'auto'
  },
  fieldLabelWrapper: {
    display: 'flex'
  },
  textfieldWrapper: {
    margin: 'auto',
    marginBottom: '15px'
  },
  helperText: {
    width: '95%',
    maxWidth: 600,
    textAlign: 'right'
  }
}));
