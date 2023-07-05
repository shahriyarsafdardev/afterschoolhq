import { Grid, TextField } from '@material-ui/core';
import React from 'react';

import useStyles from './styles';

function CheckBoxLink({
  ID,
  Link,
  index,
  question,
  IsLocked,
  setQuestion,
  customQuestion,
  QuestionDataTypeId
}) {
  const classes = useStyles();
  return (
    <Grid item lg={12} md={12} sm={12} xs={12} container>
      <Grid lg={1}></Grid>
      <Grid lg={11} md={12} sm={12} xs={12}>
        <TextField
          className={classes.formControlClass}
          size="small"
          fullWidth
          key={`linkOfQuestion-${ID}-${index}-${QuestionDataTypeId}`}
          placeholder="Link (Optional)"
          value={Link || ''}
          onChange={event => {
            setQuestion({ ...question, Link: event.target.value });
          }}
          disabled={!customQuestion || IsLocked}
          data-testid={`linkOfQuestion-${ID}-${index}`}
        />
      </Grid>
    </Grid>
  );
}

export default CheckBoxLink;
