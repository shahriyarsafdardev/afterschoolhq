import { makeStyles } from '@material-ui/core';

export default makeStyles(theme => ({
  dialogButton: {
    fontSize: '12px',
    minWidth: '20%',
    fontWeight: 'bold',
    background: theme.palette.primary.main,
    color: '#FFFFFF',
    '&:hover': {
      color: theme.palette.primary.main
    }
  },
  dialogTitle: {
    background: theme.palette.primary.main,
    color: '#FFFFFF',
    '& .MuiTypography-root': {
      color: '#FFFFFF',
      paddingLeft: '15px'
    }
  },
  dialogWrapper: {
    '& .MuiPaper-root': {
      padding: 0
    }
  },
  dialogContentText: { fontSize: '16px', padding: '20px', textAlign: 'center' },
  dialogActions: { display: 'flex', justifyContent: 'center' },
  addNewSectionButton: {
    color: '#494949',
    fontSize: '14px',
    fontWeight: 'bold',
    marginTop: '25px',
    border: '1px solid #E4E4E4'
  },
  warningButton: {
    color: '#494949 !important',
    background: '#FFFFFF !important',
    border: '1px solid #DEDEDE'
  }
}));
