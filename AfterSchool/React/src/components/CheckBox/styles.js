import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  checkboxWrapper: {
    borderRadius: 12,
    display: 'flex',
    maxWidth: 515,
    overflowWrap: 'anywhere',
    marginLeft: '14px',
    width: '100%',
    marginBottom: 15
  },
  formControlLabelRoot: {
    margin: '10px 0px',
    alignItems: 'flex-start',
    '& .MuiTypography-root': {
      marginLeft: 18
    }
  },
  checked: {
    color: '#494949 !important',
    padding: 0,
    borderRadius: 3,
    background: 'transparent !important'
  },
  root: {
    padding: 0,
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
  textFieldGrid: {
    maxWidth: 540,
    [theme.breakpoints.down('sm')]: {
      width: '100%'
    }
  },
  titleGrid: {
    [theme.breakpoints.down('sm')]: {
      width: '100%'
    }
  },
  textFieldBox: {
    display: 'flex',
    alignItems: 'center',
    [theme.breakpoints.down('sm')]: {
      flexDirection: 'column',
      justifyContent: 'flex-start'
    }
  },
  flexCenter: {
    display: 'flex',
    paddingLeft: 25
  },
  maxWidth: {
    width: '100%',
    maxWidth: 500
  }
}));

export default useStyles;
