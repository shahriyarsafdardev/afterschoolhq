import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  wrapper: { display: 'flex', maxWidth: '675px' },
  audioWrapper: {
    marginTop: 30,
    paddingTop: '8px',
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    fontFamily: 'Montserrat',
    color: theme.palette.text.primary,
    [theme.breakpoints.down('sm')]: {
      marginLeft: 0
    }
  },
  pauseButton: {
    color: theme.palette.primary.main,
    width: 91
  },
  recordButton: {
    borderRadius: 10,
    fontFamily: 'Montserrat',
    fontWeight: 300,
    fontSize: 18,
    color: '#00AE84',
    marginTop: 70,
    boxShadow: '0px 4px 13px #bababa',
    width: 'fit-content',
    minWidth: 265,
    padding: theme.spacing(1, 3),
    [theme.breakpoints.down('xs')]: {
      marginTop: 50
    }
  },
  audioWave: {
    width: 675,

    [theme.breakpoints.down('sm')]: {
      width: '100%'
    }
  },
  descriptionWrapper: {
    display: 'flex',
    flexDirection: 'column',
    width: '80%',
    marginTop: 20,
    alignItems: 'center',

    [theme.breakpoints.down('xs')]: {
      width: '100%'
    }
  },
  descriptionText: {
    border: `1px solid ${theme.palette.primary.dark}`,
    borderRadius: 10,
    padding: '20px 27px',
    display: 'flex',
    flexWrap: 'wrap',
    width: '100%',

    [theme.breakpoints.down('xs')]: {
      width: 'calc(100% - 54px)'
    }
  },
  editDescriptionButton: {
    fontFamily: 'Montserrat',
    fontWeight: 300,
    fontSize: 14,
    width: 280,
    color: theme.palette.text.primary,
    marginTop: 10
  },
  audioInput: {
    display: 'none'
  },
  outerWrapper: {
    marginBottom: '15px'
  }
}));

export default useStyles;
