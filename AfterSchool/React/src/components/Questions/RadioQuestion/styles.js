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
    display: 'flex'
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
  },
  outerWrapper: {
    marginBottom: '5px'
  }
}));

export default useStyles;
