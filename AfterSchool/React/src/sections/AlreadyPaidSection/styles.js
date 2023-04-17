import { makeStyles } from '@material-ui/core';

export default makeStyles(theme => ({
  wrapper: {
    display: 'flex',
    paddingTop: '30px',
    textAlign: 'center',
    alignItems: 'center',
    flexDirection: 'column',
    justifyContent: 'space-around'
  },
  paymentWrapper: {
    display: 'flex',
    maxWidth: '700px',
    minHeight: '300px',
    textAlign: 'center',
    alignItems: 'center',
    flexDirection: 'column',
    justifyContent: 'space-around'
  },
  anotherActivityButton: {},
  viewAccountButton: {
    marginLeft: 15
  },
  link: { color: theme.palette.gray.primary },
  fontWeight500: {
    fontWeight: 500
  }
}));
