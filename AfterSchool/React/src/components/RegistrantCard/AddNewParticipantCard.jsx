import React from 'react';
import { Button, Typography, CardContent, CardActions, Card } from '@material-ui/core';

import useStyles from './styles';
import { useSelectRegistrant } from 'redux/hooks';

function AddNewParticipantCard() {
  const classes = useStyles({ disabled: true });
  const { addNewRegistrant } = useSelectRegistrant();

  const addNewParticipant = async () => await addNewRegistrant();

  return (
    <Card className={classes.root} variant="outlined">
      <CardContent className={classes.cardContent}>
        <Typography className={classes.heading} data-testid="new-participant-card-title">
          New Participant{' '}
        </Typography>
      </CardContent>
      <CardActions className={classes.actions}>
        <Button
          className={classes.enrollButton}
          onClick={addNewParticipant}
          variant="contained"
          color="primary"
          data-testid="add-new-participant-button"
        >
          Add New Participant » &nbsp;
        </Button>
      </CardActions>
    </Card>
  );
}

export default AddNewParticipantCard;
