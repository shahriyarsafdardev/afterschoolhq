import React from 'react';
import { Box, FormHelperText, RadioGroup } from '@material-ui/core';
import FieldWrapper from 'components/FieldWrapper/FieldWrapper';
import RadioButton from 'components/RadioButton';

import useStyles from './styles';

function RadioQuestion({ question, formik }) {
  const classes = useStyles();
  const { ID, Options, Text: title, IsRequired: isRequired = false } = question;
  const { touched, values, errors, setFieldValue } = formik;
  const options = Options.split(',').map(option => {
    const [key, value] = option.includes('|') ? option.split('|') : [option, option];
    return { key, value };
  });

  const handleRadioChange = ({ target: { value } }) => {
    setFieldValue(ID, JSON.stringify([{ key: value, value }]));
  };
  const value = values[ID] ? JSON.parse(values[ID])[0].value : '';
  return (
    <FieldWrapper label={title} isRequired={isRequired}>
      <div className={classes.radioGroupWrapper}>
        <div className={classes.radio}>
          <RadioGroup
            name="radio-question"
            onChange={handleRadioChange}
            value={value}
            classes={{
              root: classes.radioGroupRoot
            }}
          >
            {options.length &&
              options.map(option => (
                <RadioButton
                  key={option.key}
                  label={option.value}
                  value={option.value}
                  selectedValue={value}
                />
              ))}
          </RadioGroup>
        </div>
      </div>
      {touched[ID] && errors[ID] && (
        <Box className={classes.flexCenter}>
          <FormHelperText className={classes.maxWidth} error>
            {errors[ID]}
          </FormHelperText>
        </Box>
      )}
    </FieldWrapper>
  );
}

export default RadioQuestion;
