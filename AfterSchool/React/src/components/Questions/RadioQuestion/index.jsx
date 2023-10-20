import React from 'react';
import { Box, FormHelperText, RadioGroup } from '@material-ui/core';

import FieldWrapper from 'components/FieldWrapper/FieldWrapper';
import RadioButton from 'components/RadioButton';
import useStyles from './styles';

function RadioQuestion({ question, formik }) {
  const classes = useStyles();
  const { touched, values, errors, setFieldValue } = formik;
  const { ID, Options, Text: title, IsRequired: isRequired = false, QuestionDataTypeId } = question;
  const options = Options.split(',').map(option => {
    const [key, value] = option.includes('|') ? option.split('|') : [option, option];
    return { key, value };
  });

  const handleRadioChange = ({ target: { value } }) => {
    setFieldValue(ID, JSON.stringify([{ key: value, value }]));
  };
  const value = (() => {
    if (values[ID]) {
      const { value, Value } = JSON.parse(values[ID])[0];
      return value || Value;
    } else return '';
  })();

  return (
    <FieldWrapper label={title} isRequired={isRequired}>
      <Box className={classes.outerWrapper}>
        <div className={classes.radioGroupWrapper}>
          <div className={classes.radio}>
            <RadioGroup
              name="radio-question"
              onChange={handleRadioChange}
              value={value}
              classes={{
                root: classes.radioGroupRoot
              }}
              data-testid={`${title}-${QuestionDataTypeId}`}
            >
              {options.length &&
                options.map(option => (
                  <RadioButton
                    key={option.key}
                    label={option.value}
                    value={option.value}
                    selectedValue={value}
                    dataTestId={`${title}-${QuestionDataTypeId}-${option.value}`}
                  />
                ))}
            </RadioGroup>
          </div>
        </div>
        {touched[ID] && errors[ID] && (
          <Box className={classes.flexCenter}>
            <FormHelperText
              data-testid={`${title}-${QuestionDataTypeId}-error`}
              className={classes.errorMessages}
              error
            >
              {errors[ID]}
            </FormHelperText>
          </Box>
        )}
      </Box>
    </FieldWrapper>
  );
}

export default RadioQuestion;
