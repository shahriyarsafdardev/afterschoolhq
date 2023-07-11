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
    justifyContent: 'end'
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
    marginTop: '25px'
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
    textDecoration: 'underline',
    cursor: 'pointer'
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
    height: '50px'
  },
  fieldWrapper: {
    display: 'flex',
    alignItems: 'baseline'
  },
  checkboxPadding: {
    paddingLeft: '0px'
  }
}));
