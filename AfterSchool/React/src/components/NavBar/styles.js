import { makeStyles } from '@material-ui/core/styles';
import header from '../../assets/header.png';
import headerMobile from '../../assets/header-mob.png';

const useStyles = makeStyles(theme => ({
  headerWrapper: {
    background: '#454E51',
    '& .MuiButton-label': {
      color: 'white'
    },
    '& .MuiButton-contained': {
      background: '#787E81'
    }
  },
  outerOption: {
    display: 'flex'
  },
  option: {
    display: 'none',
    [theme.breakpoints.up('md')]: {
      display: 'flex'
    }
  },
  menu: {
    display: 'flex',
    [theme.breakpoints.up('md')]: {
      display: 'none'
    }
  },
  navBar: {
    background: 'grey',
    display: 'flex',
    justifyContent: 'space-between'
  },
  navOptions: {
    display: 'flex'
  },
  navImage: {
    marginTop: '30px',
    height: '165px',
    padding: '0px 40px 15px 40px',
    backgroundImage: `url(${header})`,
    backgroundSize: 'cover',
    gap: '8px',
    [theme.breakpoints.down('sm')]: {
      backgroundImage: `url(${headerMobile})`
    },
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'center'
  },
  organizationName: {
    fontSize: '16px',
    color: '#ffffff',
    [theme.breakpoints.down('sm')]: {
      fontSize: '14px'
    }
  },
  activityName: {
    fontSize: '24px',
    color: '#ffffff',
    [theme.breakpoints.down('sm')]: {
      fontSize: '20px'
    }
  }
}));

export default useStyles;
