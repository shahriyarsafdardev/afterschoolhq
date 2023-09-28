import { makeStyles } from '@material-ui/core';

export default makeStyles(theme => ({
  sidebar: {
    height: 365,
    width: 287,
    padding: 20,
    border: `1px solid ${theme.palette.primary.main}`,
    borderRadius: 10,
    boxShadow: '0px 4px 10px rgba(0, 0, 0, 0.1)',
    position: 'absolute',
    top: 160,
    right: 50,
    [theme.breakpoints.down('lg')]: {
      display: 'none'
    }
  },
  sidebarHeader: {
    textAlign: 'center',
    color: theme.palette.primary.main
  },
  sectionsListWrapper: {
    gap: 10,
    margin: '20px 0 20px 0',
    display: 'flex',
    flexDirection: 'column',
    height: 250,
    overflowY: 'auto',
    textAlign: 'left',
    '&::-webkit-scrollbar': {
      width: 7
    },
    '&::-webkit-scrollbar-thumb': {
      background: theme.palette.text.hint,
      borderRadius: 6
    }
  },
  sectionName: {
    color: theme.palette.text.primary
  },
  sectionItemWrapper: {
    gap: 10,
    display: 'flex',
    alignItems: 'flex-start'
  },
  sectionItemWrapperHover: {
    '&:hover': {
      textDecoration: 'underline',
      cursor: 'pointer'
    }
  },
  icon: {
    width: 20
  },
  scrollText: {
    textAlign: 'center'
  }
}));
