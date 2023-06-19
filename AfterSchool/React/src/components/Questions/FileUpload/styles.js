import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  uploadButtonWrapper: {
    gap: 5,
    display: 'flex',
    alignItems: 'center',
    flexDirection: 'column'
  },
  photoWrapper: {
    gap: 5,
    width: '100%',
    display: 'flex',
    maxWidth: '675px',
    textAlign: 'initial',
    flexDirection: 'column'
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
    paddingLeft: 14
  },
  maxWidth: {
    width: '100%',
    maxWidth: 675
  }
}));

export default useStyles;
