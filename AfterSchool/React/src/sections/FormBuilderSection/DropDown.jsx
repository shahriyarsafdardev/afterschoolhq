import React from 'react';
import { Select, FormControl, MenuItem } from '@material-ui/core';

import useStyles from './styles';

const DropDown = ({ question, setFieldValue }) => {
  const classes = useStyles();
  const { Text: title, QuestionDataTypeId, options, QuestionTypeId } = question;
  const questionType = options.find(option => option.key === QuestionTypeId);

  const handleChange = ({ target: { value } }) => {
    const selectedOption = options.find(option => option.value === value);
    if (selectedOption) setFieldValue(selectedOption.key);
  };

  return (
    <div className={classes.formControlDropDownClass}>
      <FormControl
        variant="outlined"
        classes={{
          root: classes.formControlRoot
        }}
        data-testid={`${title}-${QuestionDataTypeId}-FormControl`}
      >
        <Select
          data-testid={`${title}-${QuestionDataTypeId}-select`}
          inputProps={{
            'data-testid': `${title}-${QuestionDataTypeId}-inputprops`
          }}
          id="dropdown-question"
          value={questionType.value}
          onChange={handleChange}
          classes={{
            select: classes.select
          }}
          MenuProps={{
            anchorOrigin: {
              vertical: 'bottom',
              horizontal: 'left'
            },
            transformOrigin: {
              vertical: 'top',
              horizontal: 'left'
            },
            getContentAnchorEl: null
          }}
        >
          <MenuItem
            key="Select"
            value="Select"
            data-testid={`${title}-${QuestionDataTypeId}-option-Select`}
          >
            Select
          </MenuItem>
          {options.length &&
            options.map(({ key, value }) => (
              <MenuItem
                key={key}
                value={value}
                classes={{
                  root: classes.list
                }}
                data-testid={`${title}-${QuestionDataTypeId}-option-${value}`}
              >
                {value}
              </MenuItem>
            ))}
        </Select>
      </FormControl>
    </div>
  );
};
export default DropDown;
