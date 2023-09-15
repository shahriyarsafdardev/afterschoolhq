import cs from 'classnames';
import Emoji from 'react-emoji-render';
import React, { useEffect, useState } from 'react';
import {
  Box,
  Button,
  IconButton,
  ClickAwayListener,
  Chip,
  FormHelperText
} from '@material-ui/core';

import { emojis } from 'constants';
import useStyles from '../../styles';
import { mapOptions } from 'sections/FormBuilderSection/helper';

const MoodQuestion = props => {
  const classes = useStyles();
  const [open, setOpen] = useState(false);
  const { errors, touched, initialized, question, setQuestion, options, setOptions } = props;
  const { ID, QuestionDataTypeId } = question;
  const [selectedEmojis, setSelectedEmojis] = React.useState(options || []);

  const emojiOptions = mapOptions(emojis);

  const emojiContent = key => (key === ':slight_frown:' ? ':slightly_frowning:' : key);

  const handleDelete = chipToDelete => {
    const updatedEmojis = [...selectedEmojis].filter(emoji => emoji.key !== chipToDelete.key);
    setSelectedEmojis(updatedEmojis);
  };

  const findEmoji = mood => selectedEmojis.findIndex(emoji => emoji.key === mood.key);

  const handleChange = mood => {
    const index = findEmoji(mood);
    if (index === -1) {
      setSelectedEmojis([...selectedEmojis, { key: mood.key, value: mood.value }]);
    } else {
      const { [index]: unselected, ...restSelectedEmojis } = selectedEmojis;
      setSelectedEmojis(Object.values(restSelectedEmojis));
    }
  };

  useEffect(() => {
    setOptions(selectedEmojis);
  }, [selectedEmojis]);

  useEffect(() => {
    if (initialized) setQuestion({ ...question, Options: options });
  }, [options]);

  return (
    <>
      <Box className={classes.moodSelector}>
        <Button
          color="primary"
          variant="outlined"
          className={classes.selectMood}
          onClick={() => setOpen(true)}
          data-testid="select-mood"
        >
          Select Mood
        </Button>
      </Box>
      {open && (
        <Box className={classes.setAudienceWrapper}>
          <ClickAwayListener onClickAway={() => setOpen(false)}>
            <Box className={cs(classes.positionAbsolute, classes.emojiOuterWrapper)}>
              <Box className={classes.emojisWrapper}>
                {emojiOptions.map((mood, index) => (
                  <Box key={index}>
                    <IconButton
                      key={mood.key}
                      onClick={() => {
                        handleChange(mood);
                      }}
                      data-testid={`emoji-selector-${mood.key}`}
                      className={findEmoji(mood) !== -1 ? classes.emoji : ''}
                    >
                      <Emoji>{emojiContent(mood.key)}</Emoji>
                    </IconButton>
                  </Box>
                ))}
              </Box>
            </Box>
          </ClickAwayListener>
        </Box>
      )}
      <Box>
        <Box className={classes.emojisList}>
          {selectedEmojis.map(emoji => {
            let icon = (icon = <Emoji>{emojiContent(emoji.key)}</Emoji>);

            return (
              <Chip
                icon={icon}
                key={emoji.key}
                label={emoji.value}
                className={classes.chip}
                classes={{ root: classes.chipRoot }}
                onDelete={() => handleDelete(emoji)}
                data-testid={`emoji-${emoji.key}`}
              />
            );
          })}
        </Box>
        {touched?.Options && errors?.Options && typeof errors?.Options === 'string' && (
          <FormHelperText
            data-testid={`${ID}-${QuestionDataTypeId}-error`}
            className={classes.errorMessage}
            error
          >
            {errors?.Options}
          </FormHelperText>
        )}
      </Box>
    </>
  );
};

export default MoodQuestion;
