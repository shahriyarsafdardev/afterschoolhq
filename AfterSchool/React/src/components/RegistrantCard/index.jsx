import React from 'react';
import { Button, Link, Typography, CardContent, CardActions, Card } from '@material-ui/core';

import useStyles from './styles';

function RegistrantCard({ student, handleClick }) {
  const {
    FirstName,
    LastName,
    isAdult,
    isStudent,
    IneligibleText,
    isParentOrGuardien,
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
      text = 'Participant';
      if (isAdult) text += ' (Adult)';
      else if (isParentOrGuardien) text += '/Guardian';
      else if (isStudent && RegistrantGradeText) text += ` (${RegistrantGradeText})`;
    }

    return text;
  };

  return (
    <Card className={classes.root} variant="outlined">
      <CardContent className={classes.cardContent}>
        <Typography className={classes.heading}>{name}</Typography>
        <Typography className={classes.subHeading}>{registrantTypeText()}</Typography>
      </CardContent>
      <CardActions className={classes.actions}>
        <Button
          className={classes.enrollButton}
          onClick={handleClick}
          disabled={disabled}
          variant="contained"
          color="primary"
        >
          {buttonText} &nbsp;
        </Button>
        <Typography className={classes.enrollMsg}>{IneligibleText}</Typography>
        {updateProfileUrl && (
          <Link
            data-testid={'Update Profile'}
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
