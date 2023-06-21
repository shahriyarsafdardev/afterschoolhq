import { makeStyles } from '@material-ui/core/styles';

export default makeStyles(theme => ({
  fieldLabel: {
    textAlign: 'left',
    fontWeight: '600',
    display: '-webkit-inline-box',
    [theme.breakpoints.down('sm')]: {
      display: 'inline-block',
      fontSize: '14px'
    }
  },
  fieldLabelOptional: {
    fontStyle: 'italic'
  },
  fieldLabelRequired: {
    color: 'red'
  },
  outerWrapper: {
    maxWidth: '675px',
    margin: 'auto'
  },
  textFieldWrapper: {
    display: 'flex'
  },
  childWrapper: {
    margin: 'auto'
  }
}));
