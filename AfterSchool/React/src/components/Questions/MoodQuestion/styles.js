import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  wrapper: {
    display: 'flex'
  },
  moodWrapper: {
    paddingTop: '8px',
    maxWidth: '675px',
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    [theme.breakpoints.down('sm')]: {
      marginLeft: 0
    }
  },
  iconsWrapper: {
    maxWidth: 675,
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
    fontSize: 14,
    color: theme.palette.text.hint
  },
  outerWrapper: {
    marginBottom: '15px'
  }
}));

export default useStyles;
