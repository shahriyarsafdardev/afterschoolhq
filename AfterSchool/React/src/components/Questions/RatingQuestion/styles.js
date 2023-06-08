import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles((theme) => ({
  formControlLabelRoot: {
    alignItems: 'center',
    marginBottom: theme.spacing(2),
    [theme.breakpoints.down('xs')]: {
      marginBottom: 30,
    },
    '&.MuiFormControlLabel-labelPlacementStart': {
      marginLeft: 0,
    },
    '& .MuiTypography-root': {
      marginBottom: 0,
      marginRight: 30,
    },
  },
  root: {
    padding: 0,
    background: theme.palette.text.disabled,
    color: theme.palette.text.disabled,
    borderRadius: '50%',
  },
  radioGroupRoot: {
    flexDirection: 'column',
    padding: '38px 0',
  },
  icon: {
    borderRadius: '50%',
    width: 24,
    height: 24,
  },
  checkedIcon: {
    backgroundColor: theme.palette.primary.dark,
    '&:before': {
      display: 'block',
      width: 24,
      height: 24,
      backgroundImage: 'radial-gradient(#fff,#fff 28%,transparent 32%)',
      content: '""',
    },
  },
  ratingWrapper: {
    display: 'flex',
    paddingLeft: 90,
    paddingTop: 60,
    paddingBottom: 90,
    flexDirection: 'column',
    alignItems: 'flex-start',
    [theme.breakpoints.down('sm')]: {
      width: 'calc(100% - 90px)',
    },
    [theme.breakpoints.down('xs')]: {
      paddingLeft: 80,
      width: 'calc(100% - 68px)',
    },
  },
  ratingText: {
    fontWeight: 300,
    fontSize: 20,
    textDecoration: 'underline',
  },
}));

export default useStyles;
