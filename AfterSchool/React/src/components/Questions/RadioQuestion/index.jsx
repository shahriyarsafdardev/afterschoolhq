import React from 'react';
import { Box, FormHelperText, RadioGroup } from '@material-ui/core';

import FieldWrapper from 'components/FieldWrapper/FieldWrapper';
import { restrictedQuestionsDataTypeIds } from 'constants';
import RadioButton from 'components/RadioButton';
import useStyles from './styles';

function RadioQuestion({ question, formik, genderRestrictions = {} }) {
  const classes = useStyles();
  const { touched, values, errors, setFieldValue } = formik;
  const { enforcedGender = 0, isGenderRestricted = false } = genderRestrictions;
  const { ID, Options, Text: title, IsRequired: isRequired = false, QuestionDataTypeId } = question;
  let options = Options.split(',').map(option => {
    const [key, value] = option.includes('|') ? option.split('|') : [option, option];
    return { key, value };
  });

  if (
    restrictedQuestionsDataTypeIds.gender === QuestionDataTypeId &&
    isGenderRestricted &&
    enforcedGender !== 0
  ) {
    if (enforcedGender === 1) {
      options = options.filter(option => option.key === 'Male');
    } else if (enforcedGender === 2) {
      options = options.filter(option => option.key === 'Female');
    }
  }

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
          <FormHelperText className={classes.maxWidth} error>
            {errors[ID]}
          </FormHelperText>
        </Box>
      )}
    </FieldWrapper>
  );
}

export default RadioQuestion;
