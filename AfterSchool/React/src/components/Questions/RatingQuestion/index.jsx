import cs from 'classnames';
import React, { useEffect, useState } from 'react';
import { RadioGroup, Radio, FormControlLabel, Box } from '@material-ui/core';

import useStyles from './styles';
import FieldWrapper from 'components/FieldWrapper/FieldWrapper';

const RatingQuestion = ({ question, formik }) => {
  const classes = useStyles();
  const { setFieldValue } = formik;
  const { ID, Options, Text: title, IsRequired: isRequired } = question;

  const options = Options.split(',').map(option => {
    const [key, value] = option.includes('|') ? option.split('|') : [option, option];
    return { key, value };
  });
  const [loading, setLoading] = useState(false);
  const [ratingValue, setRatingValue] = useState('');
  const [leastValue, setLeastValue] = useState('');
  const [highestValue, setHighestValue] = useState('');

  const handleRadioChange = ({ target: { value } }) => {
    setRatingValue(value);
    const selectedOption = options.filter(option => option.key === value);
    setFieldValue(ID, JSON.stringify(selectedOption));
  };

  const handleAnswer = async () => {
    if (!isRequired && !ratingValue) return true;
    setLoading(true);
    const selectedOption = options.filter(option => option.key === ratingValue);
    const answerFormData = new FormData();
    answerFormData.append('AnswerText', JSON.stringify(selectedOption));
    setRatingValue('');
    setLoading(false);
    return [];
  };

  useEffect(() => {
    setLeastValue(options[0].value);
    setHighestValue(options[options.length - 1].value);
  }, [options]);

  //   useEffect(() => {
  //     setRatingValue('');
  //     const availableAnswer = surveyAnswers?.find(answer => answer.uid === uid);
  //     if (availableAnswer?.answer) {
  //       const ans = JSON.parse(availableAnswer?.answer);
  //       setRatingValue(ans[0].key);
  //     }
  //     // // eslint-disable-next-line react-hooks/exhaustive-deps
  //   }, [uid]);

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
    </FieldWrapper>
  );
};

export default RatingQuestion;
