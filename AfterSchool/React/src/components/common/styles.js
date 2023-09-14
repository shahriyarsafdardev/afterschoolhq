import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  pinModalContentWrapper: {
    display: 'flex',
    padding: '8px 12px',
    flexDirection: 'column',
    maxWidth: 'max-content'
  },
  modalElementMargin: {
    margin: '8px 12px !important'
  },
  modalRoot: {
    background: theme.palette.primary.main
  },
  modal: {
    borderRadius: 10,
    maxHeight: '80vh',
    maxWidth: '1232px',
    alignItems: 'center'
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
