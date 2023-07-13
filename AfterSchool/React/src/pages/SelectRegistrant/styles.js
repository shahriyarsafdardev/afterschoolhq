import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  outerWrapper: {
    display: 'flex',
    justifyContent: 'center'
  },
  innerWrapper: {
    display: 'grid',
    gridTemplateColumns: 'auto auto auto',
    gap: '20px',
    [theme.breakpoints.up('xl')]: {
      maxWidth: '80%'
    },
    [theme.breakpoints.down('md')]: {
      gridTemplateColumns: 'auto'
    },
    [theme.breakpoints.only('lg')]: {
      gridTemplateColumns: 'auto auto',
      maxWidth: '90%'
    },
    [theme.breakpoints.only('xs')]: {
      gridTemplateColumns: 'auto'
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
