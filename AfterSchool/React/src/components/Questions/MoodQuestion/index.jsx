import React, { useEffect, useState } from 'react';
import { Box, IconButton } from '@material-ui/core';
import Emoji from 'react-emoji-render';
import cs from 'classnames';

import useStyles from './styles';
import FieldWrapper from 'components/FieldWrapper/FieldWrapper';
import ErrorBand from 'components/common/ErrorBand';

const MoodQuestion = ({ question, formik }) => {
  const classes = useStyles();
  const { touched, values, errors, setFieldValue } = formik;

  const { ID, Options, Text: title, IsRequired: isRequired } = question;
  const options = Options.split(',').map(option => {
    const [key, value] = option.includes('|') ? option.split('|') : [option, option];
    return { key, value };
  });
  const [currentMood, setCurrentMood] = useState(null);
  const [loading, setLoading] = useState(false);

  const handleMoodChange = mood => () => {
    setCurrentMood(mood);
    setFieldValue(ID, JSON.stringify(mood));
  };

  const getClassName = moodName => {
    return moodName === currentMood?.value
      ? cs(classes.moodButton, classes.moodIconButton)
      : classes.moodIconButton;
  };

  const handleAnswer = async () => {
    if (!isRequired && !currentMood) return true;
    setLoading(true);
    const answerFormData = new FormData();
    answerFormData.append('AnswerText', JSON.stringify([currentMood]));
    setLoading(false);
    setCurrentMood('');
    return []; //resp;
  };

  //   useEffect(() => {
  //     setCurrentMood(null);
  //     const availableAnswer = surveyAnswers?.find(answer => answer.uid === uid);
  //     if (availableAnswer?.answer) {
  //       const ans = JSON.parse(availableAnswer?.answer);
  //       setCurrentMood(ans[0]);
  //     }
  //     // eslint-disable-next-line react-hooks/exhaustive-deps
  //   }, [uid]);

  return (
    <FieldWrapper label={title} isRequired={isRequired}>
      <Box className={classes.wrapper}>
        <div className={classes.moodWrapper}>
          <div className={classes.iconsWrapper}>
            {options.map(mood => (
              <IconButton
                key={mood.key}
                onClick={handleMoodChange(mood)}
                className={getClassName(mood.value)}
              >
                <Emoji>{mood.key}</Emoji>
              </IconButton>
            ))}
          </div>
          {currentMood && <div className={classes.moodName}>{currentMood.value}</div>}
        </div>
      </Box>
      {touched[ID] && errors[ID] && <ErrorBand error={errors[ID]} />}
    </FieldWrapper>
  );
};

export default MoodQuestion;
