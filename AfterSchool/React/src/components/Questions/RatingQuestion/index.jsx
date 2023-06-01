import cs from 'classnames';
import React, { useEffect, useState } from 'react';
import { RadioGroup, Radio, FormControlLabel, Box } from '@material-ui/core';

import useStyles from './styles';
import ErrorBand from 'components/common/ErrorBand';
import FieldWrapper from 'components/FieldWrapper/FieldWrapper';

const RatingQuestion = ({ question, formik }) => {
  const classes = useStyles();
  const { touched, values, errors, setFieldValue } = formik;

  const { ID, Options, Text: title, IsRequired: isRequired } = question;

  const options = Options.split(',').map(option => {
    const [key, value] = option.includes('|') ? option.split('|') : [option, option];
    return { key, value };
  });
  const [leastValue, setLeastValue] = useState('');
  const [highestValue, setHighestValue] = useState('');

  const handleRadioChange = ({ target: { value } }) => {
    const selectedOption = options.find(option => option.key === value);
    setFieldValue(ID, JSON.stringify(selectedOption));
  };

  const ratingValue = values[ID] && values[ID] !== null ? JSON.parse(values[ID])?.value : '';

  useEffect(() => {
    setLeastValue(options[0].value);
    setHighestValue(options[options.length - 1].value);
  }, [options]);

  return (
    <FieldWrapper label={title} isRequired={isRequired}>
      <Box>
        <div className={classes.ratingWrapper}>
          <div className={classes.ratingText}>{leastValue}</div>
          <RadioGroup
            name="radio-question"
            value={ratingValue}
            onChange={handleRadioChange}
            classes={{
              root: classes.radioGroupRoot
            }}
          >
            {options.map(rating => (
              <FormControlLabel
                key={rating.key}
                control={
                  <Radio
                    classes={{
                      root: classes.root
                    }}
                    checkedIcon={<span className={cs(classes.icon, classes.checkedIcon)} />}
                  />
                }
                label={rating.key}
                labelPlacement={'start'}
                value={rating.key}
                classes={{
                  root: classes.formControlLabelRoot
                }}
              />
            ))}
          </RadioGroup>
          <div className={classes.ratingText}>{highestValue}</div>
        </div>
      </Box>
      {touched[ID] && errors[ID] && <ErrorBand error={errors[ID]} />}
    </FieldWrapper>
  );
};

export default RatingQuestion;
