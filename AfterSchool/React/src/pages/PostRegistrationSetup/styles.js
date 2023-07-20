import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
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
  editButton: {
    cursor: 'pointer',
    textDecoration: 'underline',
    color: theme.palette.text.hint
  },
  controls: {
    margin: 'auto 0px auto auto',
    display: 'flex',
    justifyContent: 'end',
    [theme.breakpoints.down('sm')]: {
      justifyContent: 'center'
    }
  },
  cursorPointer: {
    cursor: 'pointer'
  },
  widthFull: {
    width: '100%'
  },
  pinSetupWrapper: {
    display: 'flex',
    flexDirection: 'column',
    gap: '20px'
  },
  pinSetupButtonsWrapper: {
    maxWidth: '600px',
    margin: 'auto',
    textAlign: 'center',
    paddingTop: '40px'
  },
  marginWrapper: {
    paddingTop: '25px',
    background: 'white',
    height: '100vh'
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
    width: '260px',
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
    height: '50px',
    [theme.breakpoints.down('sm')]: {
      marginLeft: 8
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
  checkboxPadding: {
    paddingLeft: '0px'
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
  }
}));
