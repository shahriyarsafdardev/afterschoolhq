import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  checkboxWrapper: {
    overflowWrap: 'anywhere',
    borderRadius: 12,
    display: 'flex',
    maxWidth: 675,
    width: '100%',
    [theme.breakpoints.up('sm')]: {
      marginLeft: '-34px',
      maxWidth: 'unset',
      width: 'unset'
    },
    [theme.breakpoints.between('sm', 'md')]: {
      marginLeft: '-24px'
    }
  },
  formControlLabelRoot: {
    margin: '10px 0px',
    alignItems: 'flex-start',
    '& .MuiTypography-root': {
      textAlign: 'left',
      marginLeft: 18,
      [theme.breakpoints.down('md')]: {
        marginLeft: 8
      }
    }
  },
  linkIcon: {
    display: 'flex',
    alignItems: 'center',
    color: theme.palette.primary.main
  },
  checked: {
    padding: 0,
    borderRadius: 3,
    color: '#494949 !important',
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
  flexCenter: {
    display: 'flex',
    paddingLeft: 14
  },
  maxWidth: {
    width: '100%',
    maxWidth: 675
  },
  outerWrapper: {
    maxWidth: '675px',
    margin: 'auto'
  },
  innerWrapper: {
    margin: 'auto auto 5px'
  },
  checkBoxIcon: {
    height: '18px',
    width: '18px'
  }
}));

export default useStyles;
