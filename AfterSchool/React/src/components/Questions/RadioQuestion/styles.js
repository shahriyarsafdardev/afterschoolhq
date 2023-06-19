import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(() => ({
  radioGroupWrapper: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    width: '100%',
    maxWidth: 675
  },
  radio: {
    width: '100%',
    display: 'flex',
    marginBottom: '5px'
  },
  radioGroupRoot: {
    flexDirection: 'row',
    justifyContent: 'start',
    maxWidth: 675,
    width: '100%'
  },
  flexCenter: {
    display: 'flex',
    paddingLeft: 14
  },
  maxWidth: {
    width: '100%',
    maxWidth: 675
  }
}));

export default useStyles;
