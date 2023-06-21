import { FormControlLabel, Checkbox, Box } from '@material-ui/core';
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
    <Box className={classes.outerWrapper}>
      <Box className={classes.innerWrapper}>
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
                checkedIcon={<CheckIcon className={classes.checkBoxIcon} />}
                iconstyle={{ fill: 'black' }}
              />
            }
            label={title}
            classes={{
              root: classes.formControlLabelRoot
            }}
            onChange={handleCheck}
          />
        </div>
      </Box>
    </Box>
  );
}

export default SimpleCheckBox;
