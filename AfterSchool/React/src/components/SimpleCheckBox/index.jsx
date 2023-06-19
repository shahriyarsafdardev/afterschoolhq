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
    <Box style={{ maxWidth: '675px', margin: 'auto' }}>
      <Box style={{ display: 'flex', margin: 'auto' }}>
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
