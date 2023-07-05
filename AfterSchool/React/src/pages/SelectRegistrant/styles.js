import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  outerWrapper: {
    display: 'flex',
    justifyContent: 'center'
  },
  innerWrapper: {
    display: 'flex',
    flexWrap: 'wrap',
    gap: '20px',
    [theme.breakpoints.up('xl')]: {
      justifyContent: 'space-between',
      maxWidth: '80%'
    },
    [theme.breakpoints.down('md')]: {
      justifyContent: 'center'
    },
    [theme.breakpoints.only('lg')]: {
      justifyContent: 'center',
      maxWidth: '80%'
    },
    [theme.breakpoints.only('xs')]: {
      // padding: '15px'
    }
  },
  heading: {
    color: '#555555',
    fontFamily: 'Montserrat',
    fontWeight: '700',
    fontSize: '22px',
    textAlign: 'center',
    margin: '50px 0px'
  }
}));

export default useStyles;
