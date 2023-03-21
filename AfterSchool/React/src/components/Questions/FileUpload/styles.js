import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles((theme) => ({
uploadButtonWrapper:{
    display:'flex',
    flexDirection:'row',
    alignItems:'center',
    maxWidth: '90%',
    justifyContent: 'space-between'
},
  photoWrapper: {
    textAlign:'initial',
    maxWidth: '540px',
    width: '100%',
    marginLeft: 12,
    paddingBottom: 15,
    display: 'flex',
    flexDirection: 'column',
    [theme.breakpoints.down('sm')]: {
      width: '100%',
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
      fontSize: 14,
    },
  },
  inputFile: {
    display: 'none',
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
