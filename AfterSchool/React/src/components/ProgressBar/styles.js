import { makeStyles } from '@material-ui/core/styles';

const useStyles = makeStyles(() => ({
  progressBar: {
    position: 'relative',
  },
  progressNumber: {
    position: 'absolute',
    color: '#ffffff',
  },
  progressWrapper: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center',
    position: 'relative',
    marginTop: '33px',
    maxWidth: '366px',
    margin: 'auto',
    '& .MuiLinearProgress-barColorPrimary': {
      borderRadius: 10,
    },
  },
}));

export default useStyles;
