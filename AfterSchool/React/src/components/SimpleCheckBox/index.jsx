import { FormControlLabel, Checkbox, Grid } from '@material-ui/core';
import CheckIcon from '@material-ui/icons/Check';
import useStyles from './styles';
import React from 'react';

function SimpleCheckBox({ value, setValue, question }) {
  const classes = useStyles();
  const { Text: title, QuestionDataTypeId } = question;

  const handleCheck = ({ target: { checked } }) => {
    setValue(checked);
  };

  return (
    <Grid className={classes.textFieldBox} xs={12}>
      <Grid xs={12} md={4} item className={classes.titleGrid} />
      <Grid xs={12} md={8} item className={classes.textFieldGrid}>
        <div className={classes.checkboxWrapper}>
          <FormControlLabel
            data-testid={`${title}-${QuestionDataTypeId}-form`}
            control={
              <Checkbox
                data-testid={`${title}-${QuestionDataTypeId}-checkbox`}
                checked={value}
                classes={{
                  checked: classes.checked,
                  root: classes.root
                }}
                checkedIcon={
                  <CheckIcon
                    style={{
                      height: '18px',
                      width: '18px'
                    }}
                  />
                }
                IconStyle={{ fill: 'black' }}
              />
            }
            label={title}
            classes={{
              root: classes.formControlLabelRoot
            }}
            onChange={handleCheck}
          />
        </div>
      </Grid>
    </Grid>
  );
}

export default SimpleCheckBox;
