import { AppBar, Button, Box, Link, MenuItem, IconButton, Typography } from '@material-ui/core';
import menu2Fill from '@iconify/icons-eva/menu-2-fill';
import { styled } from '@material-ui/core/styles';
import React, { useState, useRef } from 'react';
import { Icon } from '@iconify/react';

import MenuPopover from 'components/MenuPopover/MenuPopover';
import { useHeader } from 'redux/hooks';
import I18n from 'components/i18n/I18n';
import useStyles from './styles';

// ----------------------------------------------------------------------

const HeaderStyle = styled(AppBar)(() => ({
  backgroundColor: '#454E51',
  backdropFilter: 'blur(10px)',
  boxShadow: 'none',
  padding: '5px 10px ',
  width: '100%',
  flexDirection: 'row',
  justifyContent: 'space-between',
  height: '37px'
}));

// ----------------------------------------------------------------------

export default function NavBar() {
  const [open, setOpen] = useState(false);
  const menuRef = useRef(null);
  const {
    headerData: { Name: activityName, Organization: organizationName, HeaderImageURL },
    loading
  } = useHeader();
  const props = { headerUrl: HeaderImageURL };
  const classes = useStyles(props);

  return (
    <>
      <HeaderStyle className={classes.headerWrapper}>
        <Button size="large" variant="text" target="_blank">
          {organizationName}
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
                  <Link target="_blank" href="/">
                    {' '}
                    Return To Profile
                  </Link>
                </MenuItem>
                <MenuItem>
                  <Link target="_blank" href="/">
                    Login To Account
                  </Link>
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
          <Typography className={classes.activityName}>{activityName}</Typography>
          <Typography className={classes.organizationName}>{organizationName}</Typography>
        </Box>
      )}
    </>
  );
}
