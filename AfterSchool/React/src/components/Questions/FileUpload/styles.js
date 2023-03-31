import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  uploadButtonWrapper: {
    display: 'flex',
    maxWidth: '90%',
    alignItems: 'center',
    flexDirection: 'row',
    justifyContent: 'space-between',
    [theme.breakpoints.down('sm')]: {
      alignItems: 'start',
      flexDirection: 'column'
    }
  },
  photoWrapper: {
    height: '90px',
    width: '100%',
    marginLeft: 12,
    display: 'flex',
    maxWidth: '540px',
    paddingBottom: 15,
    textAlign: 'initial',
    flexDirection: 'column',
    justifyContent: 'space-evenly',
    [theme.breakpoints.down('sm')]: {
      width: '100%',
      height: '120px'
    }
  },
  uploadButton: {
    boxShadow: '0px 4px 13px #bababa',
    borderRadius: 10,
    fontFamily: 'Montserrat',
    fontWeight: 300,
    fontSize: 15,
    color: theme.palette.primary.main,
    padding: theme.spacing(1, 4),
    minWidth: 265,
    width: 'fit-content',
    [theme.breakpoints.down('xs')]: {
      fontSize: 14
    }
  },
  inputFile: {
    display: 'none'
  },
  flexCenter: {
    display: 'flex',
    paddingLeft: 25
  },
  maxWidth: {
    width: '100%',
    maxWidth: 500
  }
}));

export default useStyles;
