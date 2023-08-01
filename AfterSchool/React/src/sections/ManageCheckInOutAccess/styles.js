import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
  saveButton: {
    background: theme.palette.primary.main,
    fontSize: 14,
    color: '#FFFFFF',
    marginLeft: 15,
    minWidth: 95,
    [theme.breakpoints.down('xs')]: {
      width: '100%',
      marginLeft: 0
    }
  },
  editButton: {
    cursor: 'pointer',
    textDecoration: 'underline',
    color: theme.palette.text.secondary,
    textAlign: 'center'
  },
  controls: {
    margin: 'auto 0px auto auto',
    display: 'flex',
    justifyContent: 'end',
    [theme.breakpoints.down('sm')]: {
      justifyContent: 'center'
    }
  },
  innerWrapper: {
    maxWidth: '600px',
    margin: 'auto',
    textAlign: 'center',
    display: 'flex',
    flexDirection: 'column',
    gap: '20px'
  },
  alignLeft: {
    textAlign: 'left'
  },
  addPersonButton: {
    marginTop: 10,
    cursor: 'pointer',
    width: 'fit-content',
    textDecoration: 'underline',
    color: theme.palette.primary.main
  },
  listButtonWrapper: {
    display: 'flex',
    gap: '8px'
  },
  phoneField: {
    display: 'flex',
    width: '45%',
    gap: '20px',
    justifyContent: 'space-between',
    [theme.breakpoints.only('md')]: {
      width: '80%'
    },
    [theme.breakpoints.down('sm')]: {
      width: '100%'
    }
  },
  listWrapper: {
    display: 'flex',
    flexDirection: 'column',
    width: '100%'
  },
  listInnerWrapper: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'space-between',
    marginLeft: 0,
    height: '50px',
    gap: '20px',
    [theme.breakpoints.only('md')]: {
      gap: '30px'
    },
    [theme.breakpoints.down('sm')]: {
      marginBottom: 20,
      gap: 'unset',
      flexDirection: 'column',
      alignItems: 'flex-start'
    }
  },
  fieldWrapper: {
    display: 'flex',
    alignItems: 'baseline',
    marginLeft: 0,
    [theme.breakpoints.down('sm')]: {
      marginLeft: 8
    }
  },
  checked: {
    padding: 0,
    borderRadius: 3,
    color: '#494949 !important',
    background: 'transparent !important'
  },
  formControlLabelRoot: {
    '& .MuiTypography-root': {
      wordBreak: 'break-all'
    }
  },
  primaryColor: {
    color: theme.palette.text.primary
  },
  hintColor: {
    color: theme.palette.text.hint
  },
  root: {
    padding: 0,
    marginRight: 5,
    background: '#ffffff',
    color: '#ffffff',
    border: '1px solid #c4c4c4',
    borderRadius: 3,
    height: '18px',
    width: '18px',
    '& .MuiSvgIcon-root': {
      height: '18px',
      width: '18px'
    }
  },
  checkBoxIcon: {
    height: '18px',
    width: '18px'
  },
  cursorPointer: {
    cursor: 'pointer'
  },
  divider: {
    width: '100%',
    marginBottom: '10px'
  },
  authListInnerWrapper: {
    marginBottom: '50px'
  },
  studentName: {
    marginBottom: '5px',
    fontSize: '16px'
  },
  heading: {
    color: theme.palette.text.secondary
  },
  authListOuterWrapper: {
    marginTop: '85px'
  },
  listingWrapper: {
    paddingLeft: '27px',
    width: '80%',
    [theme.breakpoints.only('md')]: {
      width: '90%'
    },
    [theme.breakpoints.down('sm')]: {
      paddingLeft: 0,
      width: '100%'
    }
  },
  italicFont: {
    fontStyle: 'italic'
  },
  accessRevokedInfo: {
    fontStyle: 'italic',
    color: theme.palette.text.hint
  },
  cancelButton: {
    fontSize: 14,
    minWidth: 90,
    color: theme.palette.primary.main,
    [theme.breakpoints.down('xs')]: {
      width: '100%'
    }
  },
  buttons: {
    display: 'flex',
    justifyContent: 'center',
    [theme.breakpoints.down('xs')]: {
      flexDirection: 'column',
      gap: '10px'
    }
  },
  phoneNumber: {
    color: theme.palette.primary
  },
  fontGreen: {
    fontStyle: 'italic',
    textDecoration: 'none',
    color: theme.palette.primary.main,
    cursor: 'unset'
  },
  personInfo: {
    gap: '10px',
    display: 'flex',
    justifyContent: 'space-between',
    width: '40%',
    [theme.breakpoints.only('md')]: {
      width: '80%'
    },
    [theme.breakpoints.down('sm')]: {
      width: '100%'
    }
  }
}));
