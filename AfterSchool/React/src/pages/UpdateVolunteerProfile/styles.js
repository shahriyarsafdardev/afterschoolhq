import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
  exitButtonWrapper: {
    display: 'flex',
    justifyContent: 'flex-end'
  },
  form: {
    height: '105vh'
  },
  home: {
    height: '100vh'
  },
  progressCard: {
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'center',
    boxSizing: 'border-box',
    width: '286.43px',
    height: '300px',
    background: '#FDFDFD',
    border: `1px solid ${theme.palette.success.main}`,
    boxShadow: '0px 4px 10px rgba(0, 0, 0, 0.1)',
    borderRadius: '10px'
  },
  progressList: {
    lineHeight: '1rem',
    textAlign: 'left',
    paddingLeft: '7px'
  },
  progressListIcon: {
    fontSize: '12px'
  },
  icon: {
    width: '15px !important',
    height: '15px  !important'
  },
  fieldLabel: {
    display: 'flex',
    justifyContent: 'end'
  },
  fieldLabelOptional: {
    fontStyle: 'italic'
  },
  fieldLabelRequired: {
    color: 'red'
  },
  buttonOuterWrapper: {
    [theme.breakpoints.down('sm')]: {
      width: '100%',
      display: 'flex',
      justifyContent: 'center'
    }
  },
  buttonWrapper: {
    margin: '60px 0px',
    minWidth: '235px',
    justifyContent: 'space-between',
    display: 'flex',
    [theme.breakpoints.down('sm')]: {
      justifyContent: 'space-between',
      minHeight: '80px',
      flexDirection: 'column',
      width: '90%',
      maxWidth: 540
    }
  },
  saveButton: {
    background: theme.palette.primary.main,
    fontSize: 14,
    color: '#FFFFFF',
    marginLeft: 15,
    minWidth: 200,
    [theme.breakpoints.down('sm')]: {
      width: '100%',
      marginLeft: 0
    }
  },
  backButton: {
    fontSize: 14,
    width: 90,
    color: theme.palette.primary.main,
    [theme.breakpoints.down('sm')]: {
      width: '100%'
    }
  },
  outerWrapper: {
    minHeight: '100%',
    position: 'relative',
    background: '#ffffff',
    borderRadius: '20px',
    border: '1px solid #f9f9f9',
    boxShadow: '0 2px 4px rgb(0 0 0 / 30%)',
    padding: '15px 20px 20px',
    textAlign: 'center',
    borderBottomRightRadius: 0,
    borderBottomLeftRadius: 0,
    color: theme.palette.text.secondary
  },
  formWrapper: {
    display: 'flex',
    justifyContent: 'center',
    flexDirection: 'column',
    alignContent: 'center'
  },
  formInnerWrapper: {
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'center',
    alignItems: 'center'
  },
  flexColCentered: {
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'center',
    width: '45%'
  },
  textFieldBox: {
    display: 'flex',
    justifyContent: 'flex-end',
    alignItems: 'center'
  },
  formControlClass: {
    display: 'block',
    width: '675px',
    maxWidth: 540,
    padding: '8px 12px',
    fontSize: '13px',
    lineHeight: '1.42857143',
    color: '#777777',
    backgroundColor: '#ffffff',
    backgroundImage: 'none',
    borderRadius: '0',
    '& .MuiOutlinedInput-root': {
      width: '100%',
      height: '40px'
    }
  },
  innerContainer: {
    [theme.breakpoints.down('sm')]: {
      maxWidth: '540px'
    }
  },
  justifyContentCenter: {
    justifyContent: 'center'
  },
  leftPadding: {
    [theme.breakpoints.up('sm')]: {
      paddingLeft: '3rem'
    },
    [theme.breakpoints.down('sm')]: {
      paddingLeft: '0rem'
    }
  }
}));
