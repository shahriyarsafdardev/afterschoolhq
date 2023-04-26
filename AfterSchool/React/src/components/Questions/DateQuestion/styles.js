import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  dateWrapper: {
    display: 'block',
    maxWidth: 540,
    width: '100%',
    padding: '8px 12px',
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
    maxWidth: 540,
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
    position: 'relative'
  },
  flexCenter: {
    display: 'flex',
    paddingLeft: 25
  },
  maxWidth: {
    width: '100%',
    maxWidth: 500
  }
}));

export default useStyles;
