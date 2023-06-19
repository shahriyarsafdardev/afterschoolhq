import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  checkboxWrapper: {
    width: '100%',
    maxWidth: 675,
    display: 'flex',
    flexWrap: 'wrap',
    borderRadius: 12,
    overflowWrap: 'anywhere'
  },
  formControlLabelRoot: {
    margin: '10px 0px',
    minWidth: 'fit-content',
    marginRight: '25px',
    '& .MuiTypography-root': {
      marginLeft: 15
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
    maxWidth: 675,
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
    paddingLeft: 14,
    marginBottom: 10
  },
  maxWidth: {
    width: '100%',
    maxWidth: 675
  }
}));

export default useStyles;
