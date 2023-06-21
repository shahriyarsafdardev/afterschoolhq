import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  videoWrapper: {
    paddingTop: '8px',
    marginTop: 35,
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',

    [theme.breakpoints.down('sm')]: {
      marginLeft: 0,
      width: '100%'
    }
  },
  imgWrapper: {
    width: 675,
    height: 'auto',
    position: 'relative',
    [theme.breakpoints.down('sm')]: {
      width: '100%',
      height: 'auto'
    }
  },
  videoPlayer: {
    width: 675,
    height: '100%',
    maxHeight: 455,
    [theme.breakpoints.down('sm')]: {
      width: '100% !important',
      height: '100%',
      maxHeight: 455
    }
  },
  playButtonWrapper: {
    position: 'absolute',
    display: 'flex',
    top: 0,
    width: '100%',
    height: '100%',
    alignItems: 'center',
    justifyContent: 'center'
  },
  playButton: {
    height: 'max-content',
    cursor: 'pointer'
  },
  iconWrapper: {
    position: 'absolute',
    top: 0,
    display: 'flex',
    width: '100%',
    height: '100%',
    alignItems: 'center',
    justifyContent: 'center',
    '&:hover > button': {
      display: 'flex',
      [theme.breakpoints.down('xs')]: {
        display: 'none'
      }
    }
  },
  pauseButton: {
    display: 'none'
  },
  recordButton: {
    position: 'absolute',
    bottom: 0
  },
  orText: {
    fontFamily: 'Montserrat',
    fontSize: 14,
    color: theme.palette.text.hint,
    textTransform: 'uppercase',
    marginTop: 24
  },
  uploadButton: {
    boxShadow: '0px 4px 13px #bababa',
    borderRadius: 10,
    fontFamily: 'Montserrat',
    fontWeight: 300,
    fontSize: 18,
    color: theme.palette.primary.main,
    minWidth: 265,
    width: 'fit-content',
    marginTop: 35,
    padding: theme.spacing(1, 3)
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
    marginTop: 10,
    color: theme.palette.text.primary
  },
  videoInput: {
    display: 'none'
  },
  modal: {
    borderRadius: 15
  },
  modalContent: {
    width: 257,
    padding: '28px 29px 40px 29px',
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    fontFamily: 'Montserrat',
    fontSize: 22,
    color: theme.palette.text.primary
  },
  selectVideoButton: {
    background: theme.palette.primary.dark,
    borderRadius: 10,
    fontFamily: 'Montserrat',
    fontSize: 18,
    color: '#FFFFFF',
    marginTop: 30
  },
  outerWrapper: {
    marginBottom: '15px'
  },
  innerWrapper: {
    display: 'flex'
  },
  recordButtonWrapper: {
    display: 'flex',
    justifyContent: 'center'
  }
}));

export default useStyles;
