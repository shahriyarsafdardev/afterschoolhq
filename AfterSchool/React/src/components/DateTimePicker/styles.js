import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  dateWrapper: {
    display: 'block',
    maxWidth: 675,
    width: '100%',
    paddingTop: '8px',
    fontSize: '13px',
    lineHeight: '1.42857143',
    color: '#777777',
    backgroundColor: '#ffffff',
    [theme.breakpoints.down('sm')]: {
      width: '100%'
    },
    '&.MuiPickersDay-daySelected': {
      background: 'red'
    }
  },
  dateInput: {
    display: 'flex',
    justifyContent: 'center',
    maxWidth: 675,
    border: '1px solid #c4c4c4',

    borderRadius: 5,
    paddingLeft: '14px',
    margin: 0,
    width: '100%',
    height: '40px',
    position: 'relative',
    '& .MuiInput-underline:before': {
      borderBottom: 0
    },
    '& .MuiInput-underline:after': {
      borderBottom: 0
    },
    '& .MuiInput-underline:hover:not(.Mui-disabled):before': {
      borderBottom: 0
    },
    '& .MuiFormControl-marginNormal': {
      marginTop: 0,
      marginBottom: 0
    }
  },
  datePicker: {
    position: 'relative',
    display: 'flex',
    border: error => (error ? '1px solid #FF807D' : '1px solid rgba(0, 0, 0, 0.23)'),
    justifyContent: 'space-around',
    paddingLeft: 10,
    paddingRight: 10,
    borderRadius: 5,
    height: 40,
    alignItems: 'center',
    '& .MuiInputAdornment-positionEnd': {
      display: 'none'
    },
    '& .MuiFormControl-marginNormal': {
      marginTop: 0,
      marginBottom: 0
    },
    '& .MuiInput-underline:before': {
      content: 'unset'
    },
    '& .MuiInput-underline:after': {
      content: 'unset'
    }
  },
  flexCenter: {
    display: 'flex',
    paddingLeft: 14
  },
  error: {
    fontSize: 12,
    color: '#FF807D',
    width: '100%'
  },
  fieldLabelRequired: {
    color: '#FF807D'
  },
  fieldLabelOptional: {
    fontStyle: 'italic'
  }
}));

export default useStyles;
