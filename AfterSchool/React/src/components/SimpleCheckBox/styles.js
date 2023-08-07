import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  checkboxWrapper: {
    borderRadius: 12,
    display: 'flex',
    maxWidth: 675,
    overflowWrap: 'anywhere',
    width: '100%',
    marginBottom: 5,
    [theme.breakpoints.up('sm')]: {
      maxWidth: 'unset',
      width: 'unset'
    }
  },
  formControlLabelRoot: {
    margin: '10px 0px',
    alignItems: 'flex-start',
    '& .MuiTypography-root': {
      marginLeft: 18,
      textAlign: 'left',
      [theme.breakpoints.down('md')]: {
        marginLeft: 8
      }
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
  outerWrapper: {
    maxWidth: '675px',
    margin: 'auto'
  },
  innerWrapper: {
    display: 'flex',
    margin: 'auto'
  },
  checkBoxIcon: {
    height: '18px',
    width: '18px'
  }
}));

export default useStyles;
