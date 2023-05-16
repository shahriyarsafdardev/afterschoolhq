import { makeStyles } from '@material-ui/core';

export default makeStyles(theme => ({
  wrapper: {
    marginLeft: 'auto',
    width: '98%',
    justifyContent: 'space-between',
    [theme.breakpoints.down('sm')]: {
      flexDirection: 'column-reverse'
    }
  },
  sectionHeading: {
    // borderBottom: '1px solid #dddddd',
    // fontWeight: 'normal'
  },
  sectionName: {
    // color: '#00b492',
    fontWeight: 'normal',

    cursor: 'pointer',
    lineHeight: '2'
  },
  currentSection: {
    cursor: 'pointer',
    lineHeight: '2',
    fontWeight: 'normal',
    color: '#00b492'

    // backgroundColor: '#dedede'
  },
  sectionListWrapper: {
    // borderLeft: '1px solid #dddddd',
    paddingLeft: '10px',
    [theme.breakpoints.down('sm')]: {
      position: 'absolute',
      backgroundColor: '#F9F9F9',
      boxShadow: '4px 1px 10px rgba(0, 0, 0, 0.1)',
      zIndex: 1,
      maxHeight: '600px',
      overflow: 'scroll',
      padding: '8px 12px',
      top: '200px'
    }
  },
  formControlRoot: {
    width: '100%',
    maxWidth: 540,
    borderRadius: 10
  },
  select: {
    fontFamily: 'Montserrat'
  },
  list: {
    padding: '10px 22px',
    fontFamily: 'Montserrat',
    borderBottom: `1px solid ${theme.palette.text.disabled}`,
    '&:last-child': {
      borderBottom: 0
    }
  },
  justifyContentSpaceBetween: {
    justifyContent: 'space-between'
  },
  displayFlex: {
    display: 'flex',
    alignItems: 'center'
  },
  formControlsRow: {
    display: 'flex',
    alignItems: 'center',

    [theme.breakpoints.down('sm')]: {
      alignItems: 'unset'
    }
  },
  formControlDropDownClass: {
    // display: 'block',
    maxWidth: 340,
    width: '100%',
    padding: '8px 12px',
    fontSize: '13px',
    lineHeight: '1.42857143',
    color: '#777777',
    // backgroundColor: '#ffffff',
    // backgroundImage: 'none',
    // borderRadius: '0',
    '& .MuiOutlinedInput-root': {
      width: '100%',
      height: '40px'
    }
  },
  formControlClass: {
    // display: 'block',
    // maxWidth: 540,
    width: '100%',
    padding: '8px 12px',
    fontSize: '13px',
    lineHeight: '1.42857143',
    color: '#777777',
    // backgroundColor: '#ffffff',
    // backgroundImage: 'none',
    // borderRadius: '0',
    '& .MuiOutlinedInput-root': {
      width: '100%'
      //   height: '40px'
    }
  },
  sectionsListHeading: {
    cursor: 'pointer',
    textDecoration: 'underline',
    marginBottom: '30px'
  },
  settingsLink: {
    cursor: 'pointer',
    textDecoration: 'underline',
    fontWeight: 'normal',
    marginBottom: '30px'
  },
  applyAllRow: {
    fontWeight: 'bold'
  },
  question: {
    justifyContent: 'space-evenly',
    border: '1px solid #E4E4E4',
    flexDirection: 'row',
    borderRadius: '3px',
    margin: '30px 0px',
    display: 'flex',
    [theme.breakpoints.down('sm')]: {
      flexDirection: 'column'
    }
  },
  buttonOuterWrapper: {
    display: 'flex',
    justifyContent: 'center',
    [theme.breakpoints.down('sm')]: {
      width: '100%',
      display: 'flex',
      justifyContent: 'center'
    }
  },
  buttonWrapper: {
    margin: '60px 0px',
    width: '300px',
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
  audienceButton: {
    fontSize: 14,
    width: 120,
    height: '40px',
    border: '1px solid #E4E4E4',
    margin: '8px 12px'
    // [theme.breakpoints.down('sm')]: {
    //     width: '100%'
    // }
  },
  formSettingsButton: {
    fontSize: 14,
    width: 120,
    border: '1px solid #E4E4E4',
    // margin: '8px 12px',
    [theme.breakpoints.down('sm')]: {
      width: '100%'
    }
  },
  positionRelative: {
    position: 'relative'
  },
  positionAbsolute: {
    position: 'absolute'
  },
  audienceList: {
    border: '1px solid #E4E4E4',
    borderRadius: '3px',
    marginTop: '-45px',
    marginLeft: '110px',
    backgroundColor: '#F9F9F9',
    boxShadow: '4px 1px 10px rgba(0, 0, 0, 0.1)',
    minWidth: '350px',
    padding: '8px 12px',
    zIndex: 1,
    [theme.breakpoints.down('sm')]: {
      marginLeft: '-110px',
      minWidth: '250px'
    }
  },
  audienceRow: {
    margin: '4px 5px'
  },
  flexDirectionCol: {
    flexDirection: 'column'
  },
  questionFieldsWrapper: {
    flexBasis: '85%'
  },
  width100: {
    width: '100%'
  },
  deleteIconHidden: {
    visibility: 'hidden'
  },
  deleteIcon: {
    '&:hover > .delete-icon-visible': {
      visibility: 'unset'
    }
  },
  applyToAllControlsWrapper: {
    padding: '5px'
  },
  applyToAllControlsContainer: {
    maxWidth: '340px',
    [theme.breakpoints.down('sm')]: {
      maxWidth: '235px'
    }
  },
  applyToAllControlsItem: { padiing: '5px', display: 'flex', alignItems: 'center' },
  controlsContainer: { display: 'block', maxWidth: '454px', marginTop: '10px' },
  questionIconsWrapper: {
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'flex-start',
    padding: '8px 0px',
    flexBasis: '10%',
    [theme.breakpoints.down('sm')]: {
      flexDirection: 'row-reverse',
      justifyContent: 'space-between'
    }
  },
  optionsDragIcon: {
    [theme.breakpoints.down('sm')]: {
      alignItems: 'baseline',
      flexDirection: 'column'
    }
  },
  newSectionName: {
    padding: '8px 0px'
  },
  warningButton: {
    color: theme.palette.error.main,
    borderColor: theme.palette.error.main
  },
  dialogButton: {
    fontSize: '12px'
  }
}));
