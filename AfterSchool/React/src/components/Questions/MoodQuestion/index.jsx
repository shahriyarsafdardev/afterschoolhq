import React from 'react';
import { Box, IconButton } from '@material-ui/core';
import Emoji from 'react-emoji-render';
import cs from 'classnames';

import useStyles from './styles';
import FieldWrapper from 'components/FieldWrapper/FieldWrapper';
import ErrorBand from 'components/common/ErrorBand';

const MoodQuestion = ({ question, formik }) => {
  const classes = useStyles();
  const defaultValue = { key: '', value: '' };
  const { touched, values, errors, setFieldValue } = formik;

  const { ID, Options, Text: title, IsRequired: isRequired } = question;
  const options = Options.split(',').map(option => {
    const [key, value] = option.includes('|') ? option.split('|') : [option, option];
    return { key, value };
  });

  const handleMoodChange = mood => () => {
    setFieldValue(ID, JSON.stringify([mood]));
  };

  const currentMood = (() => {
    if (values[ID] && values[ID] !== null) {
      try {
        return JSON.parse(values[ID])[0];
      } catch (e) {
        return defaultValue;
      }
    } else return defaultValue;
  })();

  const getClassName = moodName => {
    return moodName === currentMood?.value
      ? cs(classes.moodButton, classes.moodIconButton)
      : classes.moodIconButton;
  };

  return (
    <FieldWrapper label={title} isRequired={isRequired}>
      <Box className={classes.outerWrapper}>
        <Box className={classes.wrapper}>
          <div className={classes.moodWrapper}>
            <div className={classes.iconsWrapper}>
              {options.map((mood, index) => (
                <Box key={index}>
                  <IconButton
                    key={mood.key}
                    onClick={handleMoodChange(mood)}
                    className={getClassName(mood.value)}
                  >
                    <Emoji>{mood.key}</Emoji>
                  </IconButton>
                  {currentMood.value === mood.value && (
                    <Box className={classes.moodName}>{currentMood.value}</Box>
                  )}
                </Box>
              ))}
            </div>
          </div>
        </Box>
        {touched[ID] && errors[ID] && <ErrorBand error={errors[ID]} />}
      </Box>
    </FieldWrapper>
  );
};

export default MoodQuestion;
