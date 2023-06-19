import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  wrapper: {
    display: 'flex'
  },
  photoWrapper: {
    padding: '8px 0px',
    marginTop: 30,
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    [theme.breakpoints.down('sm')]: {
      marginLeft: 0,
      width: '100%'
    }
  },
  photoContent: {
    width: 675,
    maxHeight: 480,
    position: 'relative',
    [theme.breakpoints.down('sm')]: {
      width: '100%',
      height: 'auto'
    }
  },
  webCamera: {
    maxHeight: 'none'
  },
  uploadImage: {
    maxWidth: 675,
    maxHeight: 455,
    position: 'relative',
    [theme.breakpoints.down('sm')]: {
      width: '100%',
      height: 'auto'
    }
  },
  captureButton: {
    position: 'absolute',
    bottom: 0,
    width: '100%',
    textAlign: 'center'
  },
  orText: {
    fontFamily: 'Montserrat',
    fontSize: 14,
    color: theme.palette.text.hint,
    textTransform: 'uppercase',
    margin: '24px 0 0'
  },
  uploadButton: {
    boxShadow: '0px 4px 13px #bababa',
    borderRadius: 10,
    fontFamily: 'Montserrat',
    fontWeight: 300,
    fontSize: 18,
    marginTop: theme.spacing(5),
    color: theme.palette.primary.main,
    padding: theme.spacing(1, 4),
    minWidth: 265
  },
  inputFile: {
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
  selectImageButton: {
    background: theme.palette.primary.dark,
    borderRadius: 10,
    fontFamily: 'Montserrat',
    fontSize: 18,
    color: '#FFFFFF',
    marginTop: 30
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
    marginTop: 5
  }
}));

export default useStyles;
