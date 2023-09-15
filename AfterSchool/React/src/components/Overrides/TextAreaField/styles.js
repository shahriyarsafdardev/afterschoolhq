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
    margin: 'auto',
    marginBottom: '15px',
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
      width: '100%'
    },
    '& .MuiFormHelperText-root.Mui-error': {
      fontSize: 12
    }
  },
  flexCenter: {
    paddingRight: 14,
    paddingLeft: 14,
    display: 'flex',
    justifyContent: 'end'
  },
  helperText: {
    fontSize: 12,
    display: 'flex',
    justifyContent: 'space-between'
  },
  maxWidth: {
    width: '100%',
    maxWidth: 675
  }
}));
