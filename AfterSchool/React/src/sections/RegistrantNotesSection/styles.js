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
    maxWidth: '1000px',
    width: '100%',
    [theme.breakpoints.down('md')]: {
      width: '80%'
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
    margin: 'auto',
    padding: '40px',
    paddingBottom: '20px'
  },
  closeIcon: {
    cursor: 'pointer'
  },
  fieldsWrapper: {
    padding: '10px'
  },
  subFormFields: {
    display: 'flex'
  },
  controls: {
    display: 'flex',
    justifyContent: 'center',
    gap: 30,
    marginTop: 15,
    marginBottom: 35,
    [theme.breakpoints.down('xs')]: {
      gap: 15,
      paddingRight: '40px',
      paddingLeft: '40px',
      flexDirection: 'column'
    }
  },
  gridWrapper: {
    gap: 20,
    justifyContent: 'space-between',
    [theme.breakpoints.down('sm')]: {
      gap: 15
    }
  },
  headerText: {
    color: 'white'
  },
  divider: {
    marginBottom: '10px'
  }
}));
