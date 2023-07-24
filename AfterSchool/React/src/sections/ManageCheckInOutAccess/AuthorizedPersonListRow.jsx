import cs from 'classnames';
import React from 'react';
import { Box, Typography } from '@material-ui/core';
import useStyles from './styles';
import { ErrorBand } from 'components';

function AuthorizedPersonListRow({ touched, person, errors, handleEdit, handleAddRemoveAccess }) {
  const classes = useStyles();
  const { ID, FirstName, LastName, Phone, HasAccess, IsPinSetup } = person;

  const sendInvite = () => {};

  const controlSections = () => {
    let sections = [];
    if (HasAccess && !IsPinSetup)
      sections.push(
        <>
          <Typography onClick={handleEdit} className={classes.editButton}>
            Edit
          </Typography>
          {ID !== -1 && (
            <>
              <Typography
                onClick={sendInvite}
                className={classes.editButton}
                key="resend-invite-button"
              >
                Resend Invite
              </Typography>
              <Typography
                onClick={() => handleAddRemoveAccess(false)}
                className={classes.editButton}
                key="cancel-invite-button"
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
        >
          Remove Access
        </Typography>
      );
    if (!HasAccess)
      sections.push(
        <Typography onClick={() => handleAddRemoveAccess(true)} className={classes.editButton}>
          Restore Access
        </Typography>
      );
    return sections;
  };

  return (
    <>
      <Box className={classes.listWrapper}>
        <Box className={classes.listInnerWrapper}>
          <Typography
            variant="body1"
            className={cs(classes.formControlLabelRoot, !HasAccess && classes.hintColor)}
          >
            {`${FirstName} ${LastName}`}{' '}
            {!HasAccess && (
              <Typography component="span" variant="body1" className={classes.accessRevokedInfo}>
                (Access Revoked)
              </Typography>
            )}
          </Typography>

          <Box className={classes.phoneField}>
            <Typography className={cs(classes.phoneNumber, !HasAccess && classes.hintColor)}>
              {Phone}
            </Typography>
            {controlSections()}
          </Box>
        </Box>
      </Box>
      {touched && errors?.Phone && <ErrorBand error={errors?.Phone} />}
    </>
  );
}

export default AuthorizedPersonListRow;
