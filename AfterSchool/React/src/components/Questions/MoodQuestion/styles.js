import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  wrapper: {
    display: 'flex'
  },
  moodWrapper: {
    padding: '8px 12px',
    maxWidth: '540px',
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    [theme.breakpoints.down('sm')]: {
      marginLeft: 0
    }
  },
  iconsWrapper: {
    maxWidth: 520,
    display: 'flex',
    flexWrap: 'wrap'
  },
  moodButton: {
    background: 'radial-gradient(circle 40px, #00AE84, #FFFFFF)'
  },
  moodIconButton: {
    '& img': {
      width: 60,
      height: 60
    }
  },
  moodIcon: {
    height: 60,
    width: 60
  },
  moodName: {
    fontFamily: 'Montserrat',
    fontSize: 20,
    color: theme.palette.text.hint,
    marginTop: 70,
    [theme.breakpoints.down('xs')]: {
      marginTop: 20
    }
  }
}));

export default useStyles;
