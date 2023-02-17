import React, { useState, useRef } from 'react';
// material
import {
  AppBar, Button, Box, Link, MenuItem, IconButton, Typography,
} from '@material-ui/core';
import { styled } from '@material-ui/core/styles';
import { Icon } from '@iconify/react';
import menu2Fill from '@iconify/icons-eva/menu-2-fill';
import MenuPopover from '../MenuPopover/MenuPopover';
import I18n from '../i18n/I18n';
import useStyles from './styles';

import { useRegistration } from '../../redux/hooks';

// ----------------------------------------------------------------------

const HeaderStyle = styled(AppBar)(() => ({
  backgroundColor: '#454E51',
  backdropFilter: 'blur(10px)',
  boxShadow: 'none',
  padding: '5px 10px ',
  width: '100%',
  flexDirection: 'row',
  justifyContent: 'space-between',
  height: '37px',
}));

// ----------------------------------------------------------------------

export default function NavBar() {
  const [open, setOpen] = useState(false);
  const menuRef = useRef(null);
  const classes = useStyles();
  const {
    loading, questions, questionsGroups,
    answeredQuestions, loadFormData, submitFormSection,
    activity,
  } = useRegistration();

  return (
    <>
      <HeaderStyle className={classes.headerWrapper}>
        <Button size="large" variant="text" target="_blank">
          {activity.Organization}
        </Button>
        <Box className={classes.outerOption}>
          <I18n />
          {false && ( // TODO: NavBar options are disabled for now
          <>
            <Box className={classes.option}>
              <Button variant="text" size="small" target="_blank">
                Return To Profile
              </Button>
              <Button size="small" variant="text" target="_blank">
                Login To Account
              </Button>
            </Box>

            <MenuPopover
              open={open}
              onClose={() => {
                setOpen(false);
              }}
              anchorEl={menuRef.current}
            >
              <MenuItem>
                <Link target="_blank" href="/"> Return To Profile</Link>
              </MenuItem>
              <MenuItem>
                <Link target="_blank" href="/">Login To Account</Link>
              </MenuItem>
            </MenuPopover>
            <IconButton
              aria-label="more"
              aria-controls="long-menu"
              aria-haspopup="true"
              onClick={() => setOpen(!open)}
              ref={menuRef}
              className={classes.menu}
            >
              <Icon icon={menu2Fill} />
            </IconButton>
          </>
          )}
        </Box>

      </HeaderStyle>

      {!loading && (
      <Box className={classes.navImage}>
        <Typography className={classes.activityName}>{activity.Name}</Typography>
        <Typography className={classes.organizationName}>{activity.Organization}</Typography>
      </Box>
      )}
    </>
  );
}
