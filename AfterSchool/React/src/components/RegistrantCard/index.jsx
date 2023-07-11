import React from 'react';
import DoubleArrowIcon from '@material-ui/icons/DoubleArrow';
import { Button, Link, Typography, CardContent, CardActions, Card } from '@material-ui/core';
import useStyles from './styles';

function RegistrantCard({ name, showMsg, eligible = true }) {
  const classes = useStyles({ eligible });
  const handleClick = () => {};
  let buttonText = '';
  if (name && eligible) {
    buttonText = 'Enroll';
  } else if (!eligible) {
    buttonText = 'Ineligible';
  } else {
    buttonText = 'Add New Participant';
  }

  return (
    <Card className={classes.root} variant="outlined">
      <CardContent className={classes.cardContent}>
        <Typography className={classes.heading}>{name ? name : 'New Participant'}</Typography>
        {name && (
          <Typography className={classes.subHeading}>
            Parent/Guardian Participant (Adult)
          </Typography>
        )}
      </CardContent>
      <CardActions className={classes.actions}>
        <Button
          color="primary"
          className={classes.enrollButton}
          variant="contained"
          onClick={handleClick}
          disabled={!eligible}
        >
          {buttonText} &nbsp;
          {eligible && <DoubleArrowIcon className={classes.arrowIcon} />}
        </Button>
        {showMsg && (
          <Typography className={classes.enrollMsg}>
            This activity is for students who are 16-18 years old.
          </Typography>
        )}

        {name && (
          <Link href="#" className={classes.link} variant="body1" data-testid={'Update Profile'}>
            Update Profile
          </Link>
        )}
      </CardActions>
    </Card>
  );
}

export default RegistrantCard;
