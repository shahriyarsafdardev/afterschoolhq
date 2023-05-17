import React from 'react';
import Select from 'react-select';
import { startCase } from 'lodash';
import { FormControl, Box } from '@material-ui/core';

import useStyles from './styles';

const DropDown = ({ question, setFieldValue, disabled, dataTestId }) => {
  const classes = useStyles();
  const { options, QuestionTypeId } = question;
  const questionType = options.find(option => option.key === QuestionTypeId);

  const handleChange = ({ value }) => {
    const selectedOption = options.find(option => option.key === value);
    if (selectedOption) setFieldValue(selectedOption.key);
  };

  const dropdownOptions = options.map(({ key, value }) => {
    return { value: key, label: startCase(value) };
  });

  const selectedValue = { value: questionType.key, label: startCase(questionType.value) };

  return (
    <Box className={classes.formControlDropDownClass}>
      <FormControl
        variant="outlined"
        classes={{
          root: classes.formControlRoot
        }}
        data-testid={`${dataTestId}-FormControl`}
        disabled={disabled}
      >
        <Select
          data-testid={`${dataTestId}-select`}
          options={dropdownOptions}
          value={selectedValue}
          onChange={e => {
            handleChange(e);
          }}
          isDisabled={disabled}
        />
      </FormControl>
    </Box>
  );
};
export default DropDown;
