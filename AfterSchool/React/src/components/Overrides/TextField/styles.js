import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
  fieldLabel: {
    textAlign: 'left',
    fontWeight: '600',
    display: '-webkit-inline-box',
    [theme.breakpoints.down('sm')]: {
      fontSize: '14px',
      display: 'inline-block'
    }
  },
  fieldLabelOptional: {
    fontStyle: 'italic'
  },
  fieldLabelRequired: {
    color: 'red'
  },
  formControlClass: {
    display: 'block',
    maxWidth: 675,
    width: '100%',
    paddingTop: '8px',
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
    maxWidth: '675px'
  },
  fieldLabelWrapper: {
    display: 'flex'
  },
  textfieldWrapper: {
    display: 'flex',
    margin: 'auto'
  }
}));
