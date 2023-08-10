import React from 'react';
import { Box, Divider, Typography } from '@material-ui/core';

import useStyles from './styles';
import AuthorizedPersonListSetup from './AuthorizedPersonListSetup';

function AuthPersonsManagementSection(props) {
  const classes = useStyles();
  const { values, setFieldValue, Students } = props;

  return (
    <Box>
      <Typography variant="h6" className={classes.sectionName}>
        Manage Authorized People
      </Typography>
      <Typography className={classes.heading}>
        New authorized person(s) will be notified and prompted to create their own PIN code for
        check in and check out.
      </Typography>
      <Typography className={classes.italicFont}>
        These settings will apply to all activities.
      </Typography>
      <Divider className={classes.divider} />
      <Box className={classes.authListOuterWrapper}>
        {Object.keys(values).map(key => (
          <AuthorizedPersonListSetup
            key={key}
            studentName={Students[key]?.StudentName}
            studentUid={Students[key]?.StudentUid}
            authorizedPersons={values[key]}
            setValue={value => setFieldValue(key, value)}
          />
        ))}
      </Box>
    </Box>
  );
}

export default AuthPersonsManagementSection;
