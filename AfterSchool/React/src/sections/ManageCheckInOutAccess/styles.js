import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
  saveButton: {
    background: theme.palette.primary.main,
    fontSize: 14,
    color: '#FFFFFF',
    marginLeft: 15,
    width: 95,
    [theme.breakpoints.down('sm')]: {
      width: '100%',
      marginLeft: 0
    }
  },
  editButton: {
    cursor: 'pointer',
    textDecoration: 'underline',
    color: theme.palette.text.secondary
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
    width: 'fit-content',
    textDecoration: 'underline',
    cursor: 'pointer',
    color: theme.palette.primary.main
  },
  listButtonWrapper: {
    display: 'flex',
    gap: '8px'
  },
  phoneField: {
    display: 'flex',
    width: '412px',
    justifyContent: 'space-between'
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
    width: '75%',
    height: '50px',
    [theme.breakpoints.down('sm')]: {
      marginLeft: 8
    }
  },
  fieldWrapper: {
    display: 'flex',
    alignItems: 'baseline',
    marginLeft: 0,
    width: '75%',
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
    width: '80%'
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
    width: 90,
    color: theme.palette.primary.main,
    [theme.breakpoints.down('sm')]: {
      width: '100%'
    }
  },
  buttons: {
    display: 'flex',
    justifyContent: 'center'
  },
  phoneNumber: {
    color: theme.palette.primary
  },
  fontGreen: {
    fontStyle: 'italic',
    textDecoration: 'none',
    color: theme.palette.primary.main,
    cursor: 'unset'
  }
}));
