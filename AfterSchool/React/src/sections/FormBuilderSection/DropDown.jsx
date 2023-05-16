import React from 'react';
import Select from 'react-select';
import { FormControl } from '@material-ui/core';

import useStyles from './styles';

const DropDown = ({ question, setFieldValue, disabled }) => {
  const classes = useStyles();
  const { Text: title, QuestionDataTypeId, options, QuestionTypeId } = question;
  const questionType = options.find(option => option.key === QuestionTypeId);

  const handleChange = ({ value }) => {
    const selectedOption = options.find(option => option.key === value);
    if (selectedOption) setFieldValue(selectedOption.key);
  };

  const dropdownOptions = options.map(({ key, value }) => {
    return { value: key, label: value };
  });

  const selectedValue = { value: questionType.key, label: questionType.value };

  return (
    <div className={classes.formControlDropDownClass}>
      <FormControl
        variant="outlined"
        classes={{
          root: classes.formControlRoot
        }}
        data-testid={`${title}-${QuestionDataTypeId}-FormControl`}
        disabled={disabled}
      >
        <Select
          options={dropdownOptions}
          value={selectedValue}
          onChange={e => {
            handleChange(e);
          }}
          isDisabled={disabled}
        />
      </FormControl>
    </div>
  );
};
export default DropDown;
