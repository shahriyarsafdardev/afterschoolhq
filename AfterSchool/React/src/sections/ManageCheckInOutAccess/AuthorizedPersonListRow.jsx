import cs from 'classnames';
import React, { useEffect, useState } from 'react';
import { Box, Typography } from '@material-ui/core';
import useStyles from './styles';
import { useSnackbar } from 'notistack';
import { useManageAPAccess } from 'redux/hooks';

const RESEND_LIMIT = 86400;

function AuthorizedPersonListRow({ person, handleEdit, studentUid, handleAddRemoveAccess }) {
  const classes = useStyles();
  const { ID, FirstName, LastName, Phone, HasAccess, IsPinSetup } = person;
  const inviteLinkSendAt = localStorage.getItem(`invited${ID}`);
  const [timeDifference, setTimeDifference] = useState(0);
  const { enqueueSnackbar } = useSnackbar();
  const cantInvite = timeDifference > 0;

  const { resendInvite } = useManageAPAccess();

  const sendInvite = async () => {
    if (cantInvite) return;
    await resendInvite(studentUid, ID).then(msgs => {
      if (msgs.length) return;
      localStorage.setItem(`invited${ID}`, new Date().toISOString());
      setTimeDifference(RESEND_LIMIT);
      enqueueSnackbar('Invite Sent', { variant: 'success' });
    });
  };

  useEffect(() => {
    const savedDateTime = localStorage.getItem(`invited${ID}`);
    const savedTime = new Date(savedDateTime);

    const updateTimeDifference = () => {
      const currentTime = new Date();
      const newTimeDifference = Math.round((currentTime - savedTime) / 1000);
      const timeLeft = RESEND_LIMIT - newTimeDifference;

      if (timeLeft >= 0) setTimeDifference(timeLeft);
    };

    updateTimeDifference();
  }, [inviteLinkSendAt]);

  const controlSections = () => {
    let sections = [];
    if (HasAccess && !IsPinSetup)
      sections.push(
        <>
          <Typography onClick={handleEdit} className={classes.editButton} data-testid="edit-button">
            Edit
          </Typography>
          {ID !== -1 && (
            <>
              <Typography
                onClick={sendInvite}
                className={cs(classes.editButton, cantInvite && classes.fontGreen)}
                key="resend-invite-button"
                data-testid="resend-invite-button"
              >
                {cantInvite ? 'Invite Sent!' : 'Resend Invite'}
              </Typography>

              <Typography
                onClick={() => handleAddRemoveAccess(false)}
                className={classes.editButton}
                key="cancel-invite-button"
                data-testid="cancel-invite-button"
              >
                Cancel Invite
              </Typography>
            </>
          )}
        </>
      );
    if (HasAccess && IsPinSetup)
      sections.push(
        <Typography
          onClick={() => handleAddRemoveAccess(false)}
          className={classes.editButton}
          key="remove-access-button"
          data-testid="remove-invite-button"
        >
          Remove Access
        </Typography>
      );
    if (!HasAccess)
      sections.push(
        <Typography
          onClick={() => handleAddRemoveAccess(true)}
          className={classes.editButton}
          data-testid="restore-invite-button"
        >
          Restore Access
        </Typography>
      );
    return sections;
  };

  return (
    <Box className={classes.listWrapper} key={`${studentUid}-${ID}-${HasAccess}`}>
      <Box className={classes.listInnerWrapper}>
        <Box className={classes.personInfo}>
          <Typography
            variant="body1"
            className={cs(classes.formControlLabelRoot, !HasAccess && classes.hintColor)}
          >
            {FirstName} {LastName}
            {!HasAccess && (
              <Typography component="span" variant="body1" className={classes.accessRevokedInfo}>
                (Access Revoked)
              </Typography>
            )}
          </Typography>
          <Typography className={cs(classes.phoneNumber, !HasAccess && classes.hintColor)}>
            {Phone}
          </Typography>
        </Box>
        <Box className={classes.phoneField}>{controlSections()}</Box>
      </Box>
    </Box>
  );
}

export default AuthorizedPersonListRow;
