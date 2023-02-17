import { makeStyles } from '@material-ui/core/styles';

export default makeStyles((theme) => ({
  fieldLabel: {
    display: 'flex',
    justifyContent: 'end',
    [theme.breakpoints.down('sm')]: {
      justifyContent: 'start',
      textAlign: 'left',
      padding: '0px 12px',
      fontSize: '14px',
    },
  },
  fieldLabelOptional: {
    fontStyle: 'italic',
  },
  fieldLabelRequired: {
    color: 'red',
  },
  textFieldGrid: {
    [theme.breakpoints.down('sm')]: {
      width: '100%',
    },
  },
  titleGrid: {
    [theme.breakpoints.down('sm')]: {
      width: '100%',
    },
  },
  textFieldBox: {
    display: 'flex',
    justifyContent: 'flex-end',
    alignItems: 'center',
    [theme.breakpoints.down('sm')]: {
      flexDirection: 'column',
      justifyContent: 'flex-start',
    },
  },
  formControlClass: {
    display: 'block',
    maxWidth: 540,
    width: '100%',
    padding: '8px 12px',
    fontSize: '13px',
    lineHeight: '1.42857143',
    color: '#777777',
    backgroundColor: '#ffffff',
    backgroundImage: 'none',
    borderRadius: '0',
    '& .MuiOutlinedInput-root': {
      width: '100%',
      height: '40px',
    },
  },
}));
