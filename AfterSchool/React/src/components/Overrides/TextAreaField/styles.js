import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
  fieldLabel: {
    textAlign: 'left',
    fontWeight: '600',
    display: '-webkit-inline-box',
    [theme.breakpoints.down('sm')]: {
      display: 'inline-block',
      fontSize: '14px'
    }
  },
  fieldLabelOptional: {
    fontStyle: 'italic'
  },
  fieldLabelRequired: {
    color: 'red'
  },
  formControlClass: {
    border: '1px solid #c4c4c4',
    marginTop: '10px',
    display: 'block',
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
    paddingRight: 14,
    paddingLeft: 14,
    display: 'flex',
    justifyContent: 'end'
  },
  helperText: {
    display: 'flex',
    justifyContent: 'space-between'
  },
  maxWidth: {
    width: '100%',
    maxWidth: 675
  },
  fieldLabelWrapper: {
    display: 'flex'
  },
  textfieldWrapper: {
    margin: 'auto',
    marginBottom: '15px'
  }
}));
