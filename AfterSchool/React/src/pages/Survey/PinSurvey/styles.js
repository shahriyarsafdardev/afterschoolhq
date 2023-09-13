import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
  header: {
    minHeight: 123,
    marginBottom: '-15px',
    backgroundColor: theme.palette.secondary.main,
    padding: '30px 60px',
    [theme.breakpoints.down('md')]: {
      paddingLeft: '5px',
      paddingRight: '5px',
      textAlign: 'center'
    }
  },
  surveyInfo: {
    color: theme.palette.white.primary
  },
  activityInfo: {
    fontWeight: 400,
    color: theme.palette.white.primary
  }
}));
