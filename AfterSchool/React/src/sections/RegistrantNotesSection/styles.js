import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
  noteListWrapper: {
    marginTop: '30px'
  },
  noteWrapper: {
    border: '1px solid #E4E4E4',
    marginBottom: '20px',
    padding: '20px',
    color: theme.palette.text.secondary
  },
  noteHeader: {
    display: 'flex',
    justifyContent: 'space-between',
    marginBottom: '30px'
  },
  subHeader: {
    display: 'flex',
    gap: '25px',
    [theme.breakpoints.down('xs')]: {
      gap: '10px'
    }
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
    width: '100%',
    marginBottom: '10px'
  },
  notesWrapper: {
    padding: '10px 150px 0px 150px',
    [theme.breakpoints.down('md')]: {
      padding: '5px 15px 0px 15px'
    }
  },
  notesStudentName: {
    fontWeight: '600',
    lineHeight: '24.38px'
  },
  notesHeading: {
    fontSize: '18px',
    fontWeight: '500',
    lineHeight: '21.94px'
  },
  studentNameDivider: {
    margin: '12px 0px 22px 0px',
    width: '100%'
  },
  headingDivider: {
    marginTop: '11px',
    width: '100%'
  },
  newNoteButton: {
    background: theme.palette.primary.main,
    padding: '9.5px 25px',
    fontSize: 14,
    color: '#FFFFFF',
    marginLeft: 10
  },
  notesSearchBar: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    [theme.breakpoints.down('sm')]: {
      flexDirection: 'column',
      gap: '10px'
    }
  },
  searchFieldWrapper: {
    [theme.breakpoints.down('sm')]: {
      display: 'contents'
    }
  },
  notesSearchField: {
    width: '240px',
    '& input::placeholder': {
      fontSize: '14px',
      fontWeight: '500'
    },
    '& .MuiOutlinedInput-input': {
      padding: '14.5px 11px'
    }
  },
  paginationBar: {
    display: 'flex',
    justifyContent: 'end'
  },
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
  cancelButton: {
    fontSize: 14,
    minWidth: 90,
    color: theme.palette.primary.main,
    [theme.breakpoints.down('xs')]: {
      width: '100%'
    }
  }
}));
