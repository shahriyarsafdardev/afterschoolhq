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
  thankYouWrapper: {
    display: 'flex',
    maxWidth: '700px',
    minHeight: '200px',
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
  shareBoxesWrapper: {
    width: '700px',
    display: 'flex',
    maxWidth: '90%',
    minHeight: '200px',
    marginTop: '30px',
    borderRadius: '6px',
    textAlign: 'center',
    alignItems: 'center',
    flexDirection: 'column',
    border: '2px solid #E4E4E4',
    justifyContent: 'space-around'
  },
  shareBox: {
    cursor: 'pointer',
    minWidth: '100px'
  },
  icon: {
    height: '2em',
    width: '2em',
    color: 'currentColor',
    fill: 'currentColor'
  },
  ShareButtonsWrapper: {
    display: 'flex'
  }
}));
