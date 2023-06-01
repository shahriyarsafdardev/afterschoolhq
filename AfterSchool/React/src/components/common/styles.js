import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
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
