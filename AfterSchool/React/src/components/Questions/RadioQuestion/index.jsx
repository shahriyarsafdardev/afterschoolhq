import React from 'react';
import { Box, FormHelperText, RadioGroup } from '@material-ui/core';
import FieldWrapper from 'components/FieldWrapper/FieldWrapper';
import RadioButton from 'components/RadioButton';

import useStyles from './styles';

function RadioQuestion({ question, formik }) {
  const classes = useStyles();
  const { ID, Options, Text: title, IsRequired: isRequired = false } = question;
  const { touched, values, errors, setFieldValue } = formik;
  const options = Options.split(',').map(option => ({ key: option, value: option }));

  const handleRadioChange = ({ target: { value } }) => {
    setFieldValue(ID, value);
  };
  return (
    <>
      <FieldWrapper label={title} isRequired={isRequired}>
        <div className={classes.radioGroupWrapper}>
          <div className={classes.radio}>
            <RadioGroup
              name="radio-question"
              onChange={handleRadioChange}
              value={values[ID] || ''}
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
                    selectedValue={formik.values[ID]}
                  />
                ))}
            </RadioGroup>
          </div>
        </div>
      </FieldWrapper>
      {touched[ID] && (
        <Box className={classes.flexCenter}>
          <FormHelperText className={classes.maxWidth} error>
            {errors[ID]}
          </FormHelperText>
        </Box>
      )}
    </>
  );
}

export default RadioQuestion;
