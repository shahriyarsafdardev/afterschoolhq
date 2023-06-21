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
    display: 'block',
    maxWidth: 675,
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
    maxWidth: '675px',
    margin: 'auto'
  },
  fieldLabelWrapper: {
    display: 'flex'
  },
  textFieldOuterWrapper: {
    display: 'flex',
    margin: 'auto'
  },
  textFieldContainer: {
    maxWidth: '675px',
    width: '100%'
  },
  searchBox: {
    display: 'flex',
    margin: 'auto'
  }
}));
