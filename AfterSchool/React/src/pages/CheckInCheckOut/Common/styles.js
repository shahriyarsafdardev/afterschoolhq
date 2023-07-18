import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
  wrapper: {
    backgroundColor: '#00517C',
    height: '100vh'
  },
  widthFull: {
    width: '100%'
  },
  innerContainer: {
    height: '70%',
    width: '70%',
    margin: 'auto',
    background: 'white',
    border: `8px solid ${theme.palette.primary.main}`,
    [theme.breakpoints.down('sm')]: {
      height: '95%',
      width: '90%'
    }
  },
  content: {
    width: '80%',
    margin: 'auto',
    display: 'flex',
    justifyContent: 'center',
    height: '100%',
    [theme.breakpoints.down('sm')]: {
      width: '90%'
    }
  },
  phoneForm: {
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'center',
    gap: 16,
    margin: 'auto',
    width: '40%',
    [theme.breakpoints.down('sm')]: {
      width: '100%'
    }
  },
  pinSetupForm: {
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'center',
    gap: 16,
    margin: 'auto',
    width: '80%',
    [theme.breakpoints.down('sm')]: {
      width: '100%',
      // height: '45%',
      marginTop: '30%'
    }
  },
  childListForm: {
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'center',
    gap: 16,
    padding: '20px 0px',
    width: '60%',
    [theme.breakpoints.down('sm')]: {
      width: '100%'
    }
  },
  buttonNext: {
    width: '150px',
    margin: '10px auto',
    [theme.breakpoints.down('sm')]: {
      width: '90%'
    }
  },
  headerText: {
    textAlign: 'center',
    fontWeight: 600
  },
  subHeaderText: {
    textAlign: 'center',
    fontWeight: 400
  },
  marginAuto: {
    margin: 'auto'
  },
  listButtonWrapper: {
    display: 'flex',
    gap: '8px',
    flexBasis: 'unset',
    [theme.breakpoints.down('sm')]: {
      height: '100px',
      flexDirection: 'column-reverse'
    }
  },
  fontBold: {
    fontSize: '16px',
    fontWeight: 'bold'
  },
  fontLight: {
    fontSize: '16px',
    fontWeight: 400
  },
  childListHeader: {
    fontSize: '20px',
    textAlign: 'center'
  },
  esignPage: {
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'center',
    gap: 16,
    margin: 'auto',
    width: '60%',
    [theme.breakpoints.down('md')]: {
      width: '90%'
    },
    '.sigCanvas': {
      width: '100%'
    }
  },
  activityRow: {
    display: 'flex',
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    [theme.breakpoints.down('sm')]: {
      flexDirection: 'column',
      alignItems: 'flex-start'
    }
  },
  childListWrapper: {
    overflow: 'hidden scroll',
    flexBasis: '100%',
    gap: 16,
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center'
  },
  formControlLabelRoot: {
    color: theme.palette.primary.main,
    width: '100px',
    '& .MuiTypography-root': {
      color: theme.palette.primary.main,
      fontSize: '12px'
    }
  },
  checked: {
    color: theme.palette.primary.main
  },
  root: {
    color: `${theme.palette.primary.main} !important`,
    '& .MuiSvgIcon-root': {
      height: '18px',
      width: '18px'
    }
  },
  checkedInMsg: {
    color: theme.palette.text.hint,
    fontStyle: 'italic'
  },
  thankyouPageWrapper: {
    display: 'flex',
    flexDirection: 'column',
    gap: '16px',
    padding: '50px 0px',
    margin: 'auto'
  },
  font18Bold: {
    fontSize: '18px',
    fontWeight: 600,
    textAlign: 'center'
  },
  font18Light: {
    fontSize: '18px',
    textAlign: 'center'
  }
}));
