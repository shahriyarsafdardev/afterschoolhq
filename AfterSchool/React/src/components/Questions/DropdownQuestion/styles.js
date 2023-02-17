import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  wrapper: {
    width: '100%'
  },
  dropdownWrapper: {
    display: 'block',
    maxWidth: 540,
    width: '100%',
    fontSize: '13px',
    lineHeight: '1.42857143',
    color: '#777777',
    backgroundColor: '#ffffff',
    backgroundImage: 'none',
    borderRadius: '0',
    '& .MuiOutlinedInput-root:hover .MuiOutlinedInput-notchedOutline': {
      border: `2px solid ${theme.palette.primary.dark}`
    },
    '& .MuiOutlinedInput-root': {
      borderRadius: 5,
      width: '100%',
      height: '40px'
    },
    '& .MuiSelect-select:focus': {
      backgroundColor: '#ffffff'
    }
  },
  dropdown: {
    width: '100%',
    display: 'flex',
    [theme.breakpoints.down('sm')]: {
      paddingLeft: 0
    },
    '& .MuiOutlinedInput-notchedOutline': {
      borderColor: theme.palette.primary.dark
    }
  },
  formControlRoot: {
    width: '100%',
    maxWidth: 540,
    borderRadius: 10
  },
  select: {
    fontFamily: 'Montserrat'
  },
  list: {
    padding: '10px 22px',
    fontFamily: 'Montserrat',
    borderBottom: `1px solid ${theme.palette.text.disabled}`,
    '&:last-child': {
      borderBottom: 0
    }
  },
  formControlClass: {
    display: 'block',
    maxWidth: 540,
    width: '100%',
    padding: '8px 12px',
    fontSize: '13px',
    lineHeight: '1.42857143',
    color: '#777777',
    backgroundColor: '#ffffff',
    backgroundImage: 'none',
    borderRadius: '0'
  },
  flexCenter: {
    display: 'flex',
    justifyContent: 'center',
    paddingLeft: 50
  },
  maxWidth: {
    width: '100%',
    maxWidth: 500
  }
}));

export default useStyles;
