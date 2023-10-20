import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  wrapper: {
    width: '100%'
  },
  dropdownWrapper: {
    height: 'fit-content',
    display: 'block',
    maxWidth: 675,
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
      height: 'fit-content'
    },
    '& .MuiSelect-select:focus': {
      backgroundColor: '#ffffff'
    },
    '& .MuiSelect-outlined.MuiSelect-outlined': {
      whiteSpace: 'inherit'
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
    maxWidth: 675,
    borderRadius: 10
  },
  select: {
    fontFamily: 'Montserrat',
    padding: '9.5px 14px'
  },
  list: {
    width: 675,
    whiteSpace: 'inherit',
    padding: '10px 22px',
    fontFamily: 'Montserrat',
    borderBottom: `1px solid ${theme.palette.text.disabled}`,
    '&:last-child': {
      borderBottom: 0
    },
    [theme.breakpoints.down('sm')]: {
      width: '100%'
    }
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
    borderRadius: '0'
  },
  flexCenter: {
    display: 'flex',
    paddingLeft: 14
  },
  errorMessages: {
    width: '100%',
    maxWidth: 675,
    whiteSpace: 'pre-line'
  },
  outerWrapper: {
    marginBottom: '15px'
  }
}));

export default useStyles;
