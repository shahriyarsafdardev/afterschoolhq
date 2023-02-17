import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  radioButtonWrapper: {},
  formControlLabelRoot: {
    margin: '10px 18px',
    marginLeft: 0,
    alignItems: 'center',
    '& .MuiTypography-root': {
      marginLeft: 18
    }
  },
  root: {
    padding: 0,
    background: '#ffffff',
    color: '#ffffff',
    border: '1px solid #c4c4c4',
    borderRadius: '50%'
  },
  icon: {
    borderRadius: '50%',
    width: 24,
    height: 24
  },
  checkedIcon: {
    backgroundColor: theme.palette.primary.dark,
    '&:before': {
      display: 'block',
      width: 24,
      height: 24,
      backgroundImage: 'radial-gradient(#fff,#fff 28%,transparent 32%)',
      content: '""'
    }
  }
}));

export default useStyles;
