import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
  noteWrapper: {
    border: '1px solid #E4E4E4',
    maxWidth: '1297px',
    margin: 'auto',
    marginBottom: '20px',
    padding: '20px',
    width: '90%',
    color: theme.palette.text.secondary
  },
  noteHeader: {
    display: 'flex',
    justifyContent: 'space-between',
    marginBottom: '30px'
  },
  subHeader: {
    display: 'flex',
    gap: '25px'
  },
  showMoreButton: {
    cursor: 'pointer',
    textAlign: 'center',
    color: theme.palette.text.hint
  },
  showMoreButtonWrapper: {
    marginTop: '10px'
  },
  options: {
    fontSize: 'small',
    cursor: 'pointer'
  },
  modal: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center'
  },
  paper: {
    backgroundColor: theme.palette.background.paper,
    maxWidth: '503px',
    [theme.breakpoints.down('xs')]: {
      width: '85%'
    }
  },
  modalHeading: {
    backgroundColor: '#00ae85',
    color: '#FFFFFF',
    padding: '20px',
    display: 'flex',
    justifyContent: 'space-between'
  },
  formWrapper: {
    margin: 'auto'
  },
  closeIcon: {
    cursor: 'pointer'
  }
}));
