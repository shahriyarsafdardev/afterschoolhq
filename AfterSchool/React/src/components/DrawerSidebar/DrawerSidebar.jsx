import React, { useState } from 'react';
import { Box, Drawer, Fab, Typography } from '@material-ui/core';
import { ShoppingCartOutlined, Close } from '@material-ui/icons';

import useStyles from './styles';
import { weekDays } from 'constants';
import { useSessionRegistration } from 'redux/hooks';

function DrawerSidebar() {
  const classes = useStyles();
  const [openDrawer, setOpenDrawer] = useState(false);
  const { checkOut } = useSessionRegistration();

  const displayScheduledDays = daysInNumber =>
    daysInNumber
      .split(',')
      .map(day => weekDays[day])
      .join(',');

  const activityRange = activity => ` ${activity.MinGrade} - ${activity.MaxGrade}`;

  return (
    <>
      {openDrawer ? (
        <Drawer
          anchor="right"
          open={openDrawer}
          className={classes.outerWrapper}
          onClose={() => setOpenDrawer(false)}
        >
          <Box className={classes.innerWrapper}>
            <Box className={classes.closeIconWrapper}>
              <Close onClick={() => setOpenDrawer(false)} className={classes.closeIconButton} />
            </Box>
            <Typography variant="h6" className={classes.header}>
              Activities
            </Typography>

            {checkOut?.map(activity => (
              <Box key={activity} className={classes.activityBox}>
                <Typography className={classes.colorWhite}>{activity.ActivityName}</Typography>
                <Typography className={classes.colorWhite}>
                  {displayScheduledDays(activity.DaysOfWeek)} &bull; {activityRange(activity)}
                </Typography>
              </Box>
            ))}
          </Box>
        </Drawer>
      ) : (
        <Fab className={classes.cartWrapper} onClick={() => setOpenDrawer(true)}>
          <Typography className={classes.colorWhite}>{checkOut?.length}</Typography>
          <ShoppingCartOutlined fontSize="large" />
          <Typography className={classes.colorWhite}>View Cart</Typography>
        </Fab>
      )}
    </>
  );
}

export default DrawerSidebar;
