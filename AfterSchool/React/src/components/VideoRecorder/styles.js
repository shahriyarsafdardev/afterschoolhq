import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  webCamWrapper: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    padding: theme.spacing(3),
    '& video': {
      borderRadius: 15,
      [theme.breakpoints.down('xs')]: {
        width: '100%',
        height: '100%'
      }
    },
    [theme.breakpoints.down('sm')]: {
      padding: theme.spacing(1)
    }
  },
  takePhoto: {
    background: theme.palette.success.main,
    color: '#FFFFFF',
    height: 50,
    borderRadius: 10,
    fontFamily: 'Montserrat',
    width: '100%',
    maxWidth: 250,
    marginTop: theme.spacing(1)
  }
}));

export default useStyles;
