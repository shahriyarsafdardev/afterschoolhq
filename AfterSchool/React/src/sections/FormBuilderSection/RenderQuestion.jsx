import cs from 'classnames';
import { isEqual, toInteger } from 'lodash';
import React, { useState } from 'react';
import CheckIcon from '@material-ui/icons/Check';
import AddOutlinedIcon from '@material-ui/icons/AddOutlined';
import FileCopyOutlinedIcon from '@material-ui/icons/FileCopyOutlined';
import DeleteOutlineOutlinedIcon from '@material-ui/icons/DeleteOutlineOutlined';
import ArrowDropDownOutlinedIcon from '@material-ui/icons/ArrowDropDownOutlined';
import RadioButtonUncheckedIcon from '@material-ui/icons/RadioButtonUnchecked';
import CheckBoxOutlineBlankIcon from '@material-ui/icons/CheckBoxOutlineBlank';
import DragIndicatorIcon from '@material-ui/icons/DragIndicator';

import {
  FormControlLabel,
  Typography,
  makeStyles,
  TextField,
  Checkbox,
  Button,
  Chip,
  Grid,
  Box
} from '@material-ui/core';

import useStyles from './styles';
import DropDown from './DropDown';
import questionType from 'constants/questionType';
import RenderAudienceList from './RenderAudienceList';
import { DragDropContext, Droppable, Draggable } from 'react-beautiful-dnd';

const RenderQuestion = ({
  index,
  value,
  setFieldValue,
  handleDelete,
  handleCopy,
  handleAddNewQuestion
}) => {
  const classes = useStyles();
  const [question, setQuestion] = useState(value);
  const [showAudience, setShowAudience] = useState(false);
  const { dropdown, radioList, checkBoxList } = questionType;
  const { ID, Text, Options, QuestionTypeId, QuestionSettings, QuestionDataTypeId } = question;
  const showOptions = [dropdown, radioList, checkBoxList].includes(QuestionTypeId);
  const audienceTypes = ['All', 'Parent', 'Student', 'Adult'];
  const questionTypesOptions = Object.keys(questionType).map(key => ({
    key: questionType[key],
    value: key
  }));

  const genOptions = Options?.split(',').map(option => {
    const [key, value] = option.split('|');
    return { key, value };
  });

  const [options, setOptions] = useState(genOptions || []);
  const customQuestion = QuestionDataTypeId === 0;

  function renderOptions() {
    const radioListIcon = radioList === QuestionTypeId;
    const checkboxListIcon = checkBoxList === QuestionTypeId;

    const handleKeyChange = (index, event) => {
      const newOptions = [...options];
      newOptions[index].key = event.target.value;
      setOptions(newOptions);
    };

    const handleValueChange = (index, event) => {
      const newOptions = [...options];
      newOptions[index].value = event.target.value;
      setOptions(newOptions);
    };

    const handleDeleteOption = index => {
      const newOptions = [...options];
      newOptions.splice(index, 1);
      setOptions(newOptions);
    };

    const handleAddNewOption = () => {
      const newOptions = [...options, { key: '', value: '' }];
      setOptions(newOptions);
    };

    const handleSubmit = () => {
      const optionsString = options
        .map(option => {
          if (option.value) {
            return `${option.key}|${option.value}`;
          } else {
            return option.key;
          }
        })
        .join(',');

      if (!isEqual(Options, optionsString)) {
        setQuestion({ ...question, Options: optionsString });
      }
    };

    const onDragEnd = result => {
      const { source, destination } = result;
      if (!destination) return;

      const temp = [...options];
      const [sourceValue] = temp.splice(source.index, 1);
      temp.splice(destination.index, 0, sourceValue);
      setOptions(temp);
    };

    const renderOption = ({ option, index, dragHandleProps }) => {
      return (
        <Grid container className={classes.deleteIcon} key={`value-${index}`}>
          <Grid item lg={6} xs={12} sm={6} className={cs(classes.displayFlex)}>
            <span {...dragHandleProps}>
              <DragIndicatorIcon style={{ float: 'left' }} />
            </span>
            {checkboxListIcon ? (
              <CheckBoxOutlineBlankIcon />
            ) : radioListIcon ? (
              <RadioButtonUncheckedIcon />
            ) : (
              <Typography variant="body1">{index}.</Typography>
            )}
            <TextField
              className={classes.formControlClass}
              fullWidth
              placeholder="Option"
              value={option.key}
              onChange={event => handleKeyChange(index, event)}
            />
          </Grid>
          <Grid item lg={4} xs={12} sm={6} key={`option-${index}`}>
            <TextField
              className={classes.formControlClass}
              fullWidth
              placeholder="Value (Optional)"
              value={option.value}
              onChange={event => handleValueChange(index, event)}
            />
          </Grid>
          <Grid
            item
            lg={2}
            xs={12}
            sm={6}
            className={cs(classes.deleteIconHidden, 'delete-icon-visible')}
          >
            <DeleteOutlineOutlinedIcon
              style={{
                color: '#FF7070',
                width: '30px',
                height: 'auto',
                margin: '4px 6px',
                cursor: 'pointer'
              }}
              onClick={() => handleDeleteOption(index)}
            />
          </Grid>
        </Grid>
      );
    };

    return (
      <Box
        className={cs(classes.flexDirectionCol)}
        onMouseLeave={() => {
          handleSubmit();
        }}
        style={{ display: 'flex', padding: '8px 12px' }}
      >
        <DragDropContext onDragEnd={onDragEnd}>
          <Droppable key="list" droppableId="droppable-options-container" type="column">
            {provided => (
              <div ref={provided.innerRef} {...provided.droppableProps}>
                {options.map(({ key, value }, index) => (
                  <Draggable
                    draggableId={`question-${ID}-option-${key}`}
                    index={toInteger(index)}
                    key={key}
                  >
                    {(provided, snapshot) => (
                      <Box
                        ref={provided.innerRef}
                        {...provided.draggableProps}
                        // {...provided.dragHandleProps}
                        className={
                          snapshot.isDragging ? classes.whiteBorder : classes.transparentBorder
                        }
                      >
                        {renderOption({
                          option: { key, value },
                          index,
                          dragHandleProps: { ...provided.dragHandleProps }
                        })}
                      </Box>
                    )}
                  </Draggable>
                ))}
                {provided.placeholder}
              </div>
            )}
          </Droppable>
        </DragDropContext>
        <Typography
          style={{ cursor: 'pointer', textDecoration: 'underline', padding: '12px 0px' }}
          onClick={() => {
            handleAddNewOption();
          }}
        >
          Add new option
        </Typography>
      </Box>
    );
  }

  return (
    <Box
      className={classes.question}
      onMouseLeave={() => {
        if (!isEqual(question, value)) {
          setFieldValue(index, question);
        }
      }}
    >
      <Typography variant="h6" style={{ margin: '8px 12px' }}>
        {index}
      </Typography>
      <Box className={classes.width100}>
        <Box className={classes.displayFlex}>
          <TextField
            className={classes.formControlClass}
            fullWidth
            variant="outlined"
            value={Text}
            helperText={`${Text?.length}/400`}
            inputProps={{
              maxLength: 400
            }}
            multiline
            onChange={event => {
              setQuestion({ ...question, Text: event.target.value });
            }}
            disabled={!customQuestion}
          />
          <DropDown
            question={{ ...question, ...{ options: questionTypesOptions } }}
            setFieldValue={key => setQuestion({ ...question, QuestionTypeId: key })}
            disabled={!customQuestion}
          />
          <DeleteOutlineOutlinedIcon
            style={{
              color: `${customQuestion ? '#FF7070' : '#494949'}`,
              width: '30px',
              height: 'auto',
              margin: '4px 6px',
              cursor: `${customQuestion ? 'pointer' : 'not-allowed'}`
            }}
            onClick={() => {
              if (customQuestion) handleDelete(index);
            }}
          />
          <FileCopyOutlinedIcon
            onClick={() => handleCopy(index)}
            style={{
              color: '#494949',
              width: '25px',
              height: 'auto',
              margin: '4px 6px',
              cursor: 'pointer'
            }}
          />
          <AddOutlinedIcon
            style={{
              color: '#494949',
              width: '30px',
              height: 'auto',
              margin: '4px 6px',
              cursor: 'pointer'
            }}
            onClick={() => handleAddNewQuestion(index)}
          />
        </Box>
        <Box className={cs(classes.displayFlex, classes.justifyContentSpaceBetween)}>
          <Box className={classes.displayFlex}>
            <Box className={classes.positionRelative}>
              <Button
                variant="outlined"
                className={classes.audienceButton}
                onClick={() => setShowAudience(!showAudience)}
              >
                Audience
                <ArrowDropDownOutlinedIcon
                  style={{ width: '20px', height: 'auto', marginLeft: '2px' }}
                />
              </Button>

              {showAudience && (
                <RenderAudienceList
                  setFieldValue={questionSettings =>
                    setQuestion({ ...question, QuestionSettings: questionSettings })
                  }
                  questionSettings={QuestionSettings}
                  setShowAudience={setShowAudience}
                />
              )}
            </Box>
            {QuestionSettings.map(({ Audience, Show, Required }) => {
              if (!(Show || Required)) return;

              return (
                <Chip
                  style={{ backgroundColor: '#F2F2F5', marginRight: '2px' }}
                  label={`${audienceTypes[Audience]}${Required ? '*' : ''}`}
                  key={Audience}
                />
              );
            })}
          </Box>
          <Box>
            <HideCheckBox
              setFieldValue={isActive => setQuestion({ ...question, IsActive: isActive })}
              question={question}
            />
          </Box>
        </Box>
        {showOptions && renderOptions()}
      </Box>
    </Box>
  );
};

const HideCheckBox = ({ question, setFieldValue }) => {
  const { Text: title, QuestionDataTypeId, IsActive: value } = question;

  const classes = makeStyles(() => ({
    checkboxWrapper: {
      padding: '0px 12px'
    },
    formControlLabelRoot: {
      margin: '10px 0px',
      '& .MuiTypography-root': {
        marginLeft: 18
      }
    },
    checked: {
      color: '#494949 !important',
      padding: 0,
      borderRadius: 3,
      background: 'transparent !important'
    },
    root: {
      padding: 0,
      background: '#ffffff',
      color: '#ffffff',
      border: '1px solid #c4c4c4',
      borderRadius: 3,
      height: '18px',
      width: '18px',
      '& .MuiSvgIcon-root': {
        height: '18px',
        width: '18px'
      }
    }
  }))();
  return (
    <div className={classes.checkboxWrapper}>
      <FormControlLabel
        data-testid={`${title}-${QuestionDataTypeId}-form`}
        control={
          <Checkbox
            data-testid={`${title}-${QuestionDataTypeId}-checkbox`}
            checked={!value}
            classes={{
              checked: classes.checked,
              root: classes.root
            }}
            checkedIcon={
              <CheckIcon
                style={{
                  height: '18px',
                  width: '18px'
                }}
              />
            }
            IconStyle={{ fill: 'black' }}
          />
        }
        label={'Hide'}
        classes={{
          root: classes.formControlLabelRoot
        }}
        onChange={() => {
          setFieldValue(!value);
        }}
      />
    </div>
  );
};

export default RenderQuestion;
