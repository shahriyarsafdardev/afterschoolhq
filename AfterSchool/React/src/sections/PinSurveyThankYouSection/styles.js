import { makeStyles } from '@material-ui/core';

export default makeStyles(() => ({
  wrapper: {
    display: 'flex',
    paddingTop: '30px',
    textAlign: 'center',
    alignItems: 'center',
    flexDirection: 'column',
    justifyContent: 'space-around'
  },
  thankYouWrapper: {
    display: 'flex',
    maxWidth: '700px',
    minHeight: '200px',
    textAlign: 'center',
    alignItems: 'center',
    flexDirection: 'column',
    justifyContent: 'space-around'
  }
}));
