import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  modal: {
    borderRadius: 10,
    maxWidth: 678
  },
  modalContentWrapper: {
    width: 650,
    padding: '28px 14px 20px',
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    fontFamily: 'Montserrat',
    color: theme.palette.text.primary,

    [theme.breakpoints.down('xs')]: {
      width: 'auto'
    }
  },
  descriptionField: {
    margin: '13px 0',

    '& .MuiOutlinedInput-multiline': {
      padding: '20px 25px'
    },
    '& .MuiOutlinedInput-input': {
      fontFamily: 'Montserrat'
    }
  },
  saveButton: {
    borderRadius: 10,
    background: theme.palette.primary.dark,
    fontFamily: 'Montserrat',
    fontSize: 18,
    color: '#FFFFFF',
    boxShadow: '0px 4px 13px #BABABA',
    width: 290
  },
  skipButton: {
    fontFamily: 'Montserrat',
    fontWeight: 300,
    fontSize: 14,
    color: theme.palette.text.primary,
    width: 290,
    marginTop: 5,
    borderRadius: 10
  }
}));

export default useStyles;
