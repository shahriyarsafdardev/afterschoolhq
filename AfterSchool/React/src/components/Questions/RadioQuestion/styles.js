import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(() => ({
  radioGroupWrapper: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    width: '100%',
    maxWidth: 540
  },
  radio: {
    width: '100%',
    display: 'flex',
    padding: '8px 12px'
  },
  radioGroupRoot: {
    flexDirection: 'row',
    justifyContent: 'start',
    maxWidth: 540,
    width: '100%'
  },
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
