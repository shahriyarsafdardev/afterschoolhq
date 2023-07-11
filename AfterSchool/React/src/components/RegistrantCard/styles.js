import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  root: {
    borderColor: ({ eligible }) =>
      eligible ? theme.palette.primary.main : theme.palette.text.disabled,
    borderRadius: '10px',
    boxShadow: '0 4px 8px 0 rgba(0, 0, 0, 0.1), 0 6px 20px 0 rgba(0, 0, 0, 0.1)',
    minWidth: '385px',
    minHeight: '215px',
    [theme.breakpoints.only('xs')]: {
      minWidth: '300px'
    }
  },
  enrollButton: {
    background: theme.palette.primary.main,
    fontSize: 14,
    color: '#FFFFFF',
    width: '240px'
  },
  link: {
    color: '#494949',
    fontFamily: 'Montserrat',
    fontWeight: '400'
  },
  heading: {
    color: theme.palette.text.primary,
    fontFamily: 'Montserrat',
    fontWeight: '400',
    fontSize: '20px',
    textAlign: 'center'
  },
  subHeading: {
    fontFamily: 'Montserrat',
    fontWeight: '300',
    fontSize: '14px',
    textAlign: 'center',
    color: theme.palette.text.hint
  },
  enrollMsg: {
    fontFamily: 'Montserrat',
    fontWeight: '300',
    fontSize: '14px',
    textAlign: 'center',
    color: theme.palette.text.hint,
    [theme.breakpoints.only('xs')]: {
      width: '260px'
    }
  },
  actions: {
    flexDirection: 'column',
    padding: '8px 16px 16px 16px',
    gap: '10px',
    [theme.breakpoints.only('xs')]: {
      minHeight: '144px'
    }
  },
  arrowIcon: {
    fontSize: '8px'
  },
  cardContent: {
    minHeight: '84px'
  }
}));

export default useStyles;
