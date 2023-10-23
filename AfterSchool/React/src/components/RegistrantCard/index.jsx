import React from 'react';
import { Button, Link, Typography, CardContent, CardActions, Card } from '@material-ui/core';

import useStyles from './styles';

function RegistrantCard({ index, student, handleClick }) {
  const {
    FirstName,
    LastName,
    isAdult,
    isStudent,
    IneligibleText,
    isParentOrGuardian,
    Disabled: disabled,
    RegistrantGradeText,
    BtnText: buttonText,
    NewParticipant = false,
    UpdateProfileLink: updateProfileUrl
  } = student;

  const name = `${FirstName} ${LastName}`;
  const classes = useStyles({ disabled: disabled || NewParticipant });
  const registrantTypeText = () => {
    let text = '';

    if (!NewParticipant) {
      if (isAdult) text = ' (Adult)';
      else if (isParentOrGuardian) text = '/Guardian';
      else if (isStudent && RegistrantGradeText) text = ` (${RegistrantGradeText})`;
      text = 'Participant' + text;
    }

    return text;
  };

  return (
    <Card className={classes.root} variant="outlined">
      <CardContent className={classes.cardContent}>
        <Typography className={classes.heading} data-testid={`registrant-name-${index}`}>
          {name}
        </Typography>
        <Typography className={classes.subHeading} data-testid={`registrant-type-text-${index}`}>
          {registrantTypeText()}
        </Typography>
      </CardContent>
      <CardActions className={classes.actions}>
        <Button
          className={classes.enrollButton}
          onClick={handleClick}
          disabled={disabled}
          variant="contained"
          color="primary"
          data-testid={`participant-button-${index}`}
        >
          {buttonText} &nbsp;
        </Button>
        <Typography className={classes.enrollMsg} data-testid={`ineligible-text-${index}`}>
          {IneligibleText}
        </Typography>
        {updateProfileUrl && (
          <Link
            data-testid={`update-profile-link-${index}`}
            className={classes.link}
            href={updateProfileUrl}
            target="_blank"
            variant="body1"
          >
            Update Profile
          </Link>
        )}
      </CardActions>
    </Card>
  );
}

export default RegistrantCard;
