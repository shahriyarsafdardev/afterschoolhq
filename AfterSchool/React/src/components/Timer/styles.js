import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  wrapper: {
    fontFamily: 'Montserrat',
    fontSize: 32,
    color: theme.palette.primary.light,
    marginTop: 30
  }
}));

export default useStyles;
