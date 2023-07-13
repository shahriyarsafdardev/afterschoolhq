import React from 'react';
import { Button, Link, Typography, CardContent, CardActions, Card } from '@material-ui/core';

import useStyles from './styles';

function RegistrantCard({ student, handleClick }) {
  const {
    FirstName,
    LastName,
    IneligibleText,
    RegistrantGradeText,
    BtnText: buttonText,
    Disabled: disabled,
    NewParticipant = false,
    UpdateProfileLink: updateProfileUrl
  } = student;
  const name = `${FirstName} ${LastName}`;
  const classes = useStyles({ disabled: disabled || NewParticipant });

  return (
    <Card className={classes.root} variant="outlined">
      <CardContent className={classes.cardContent}>
        <Typography className={classes.heading}>{name}</Typography>
        <Typography className={classes.subHeading}>{RegistrantGradeText}</Typography>
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
