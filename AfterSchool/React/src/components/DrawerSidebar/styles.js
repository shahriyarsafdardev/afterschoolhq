import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
  activityBox: {
    display: 'flex',
    flexDirection: 'column',
    border: `1px solid ${theme.palette.success.main}`,
    margin: 10,
    padding: 5
  },

  innerWrapper: {
    display: 'flex',
    flexDirection: 'column'
  },
  outerWrapper: {
    '& .MuiPaper-root': {
      backgroundColor: '#2F3C40',
      width: '250px',
      left: 'auto',
      right: 0
    },
    '& .MuiBackdrop-root': {
      backgroundColor: 'unset'
    }
  },
  closeIconButton: {
    cursor: 'pointer',
    color: theme.palette.white.primary
  },
  colorWhite: {
    color: theme.palette.white.primary
  },
  closeIconWrapper: {
    display: 'flex',
    justifyContent: 'end',
    margin: 5
  },
  header: {
    color: theme.palette.white.primary,
    margin: 10
  },
  cartWrapper: {
    backgroundColor: theme.palette.success.main,
    borderRadius: '15%',
    width: 101,
    color: theme.palette.white.primary,
    position: 'fixed',
    top: 50,
    right: 10,
    height: 'auto',
    '&:hover': {
      backgroundColor: theme.palette.success.main
    },
    '& .MuiFab-label': {
      display: 'block'
    }
  }
}));
