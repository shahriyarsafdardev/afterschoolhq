import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles(theme => ({
  formControlLabelRoot: {
    alignItems: 'center',
    marginBottom: theme.spacing(2),
    '&.MuiFormControlLabel-labelPlacementStart': {
      rowGap: '24px',
      margin: '0',
      flexDirection: 'column-reverse'
    },
    '& .MuiTypography-root': {
      marginBottom: 0
    }
  },
  root: {
    padding: 0,
    background: theme.palette.text.disabled,
    color: theme.palette.text.disabled,
    borderRadius: '50%'
  },
  radioGroupRoot: {
    gap: '3px',
    padding: '0 38px',
    flexDirection: 'row',
    justifyContent: 'space-evenly',
    width: '-webkit-fill-available'
  },
  icon: {
    width: 24,
    height: 24,
    borderRadius: '50%'
  },
  checkedIcon: {
    backgroundColor: theme.palette.primary.dark,
    '&:before': {
      display: 'block',
      width: 24,
      height: 24,
      backgroundImage: 'radial-gradient(#fff,#fff 28%,transparent 32%)',
      content: '""'
    }
  },
  ratingWrapper: {
    display: 'flex',
    justifyContent: 'space-between',
    padding: '20px 0 15px',
    alignItems: 'flex-start',
    maxWidth: 675
  },
  ratingText: {
    fontWeight: 300,
    fontSize: 14,
    alignSelf: 'end'
  }
}));

export default useStyles;
