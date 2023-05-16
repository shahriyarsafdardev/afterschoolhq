import cs from 'classnames';
import { toInteger } from 'lodash';
import CheckIcon from '@material-ui/icons/Check';
import React, { useEffect, useState } from 'react';
import AddOutlinedIcon from '@material-ui/icons/AddOutlined';
import DragIndicatorIcon from '@material-ui/icons/DragIndicator';
import FileCopyOutlinedIcon from '@material-ui/icons/FileCopyOutlined';
import RadioButtonUncheckedIcon from '@material-ui/icons/RadioButtonUnchecked';
import CheckBoxOutlineBlankIcon from '@material-ui/icons/CheckBoxOutlineBlank';
import DeleteOutlineOutlinedIcon from '@material-ui/icons/DeleteOutlineOutlined';
import ArrowDropDownOutlinedIcon from '@material-ui/icons/ArrowDropDownOutlined';

import {
  FormControlLabel,
  FormHelperText,
  useMediaQuery,
  Typography,
  makeStyles,
  TextField,
  useTheme,
  Checkbox,
  Button,
  Chip,
  Grid,
  Box
} from '@material-ui/core';

import Dialog from './Dialog';
import useStyles from './styles';
import DropDown from './DropDown';
import questionType from 'constants/questionType';
import RenderAudienceList from './RenderAudienceList';
import { DragDropContext, Droppable, Draggable } from 'react-beautiful-dnd';

const RenderQuestion = ({
  index,
  value,
  errors,
  touched,
  handleCopy,
  handleDelete,
  setFieldValue,
  dragIndicatorProps,
  handleAddNewQuestion
}) => {
  const theme = useTheme();
  const classes = useStyles();
  const [question, setQuestion] = useState(value);
  const [showAudience, setShowAudience] = useState(false);
  const { dropdown, radioList, checkBoxList, textBlock } = questionType;
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const {
    ID,
    Text,
    Options,
    IsLocked,
    QuestionTypeId,
    QuestionSettings,
    QuestionDataTypeId
  } = question;
  const showOptions = [dropdown, radioList, checkBoxList].includes(QuestionTypeId);
  const audienceTypes = ['All', 'Parent', 'Student', 'Adult'];
  const questionTypesOptions = Object.keys(questionType).map(key => ({
    key: questionType[key],
    value: key
  }));

  const [initialized, setInitialized] = useState(false);

  useEffect(() => {
    setInitialized(true);
  }, []);

  useEffect(() => {
    if (initialized) {
      updateValue();
    }
  }, [question]);

  const [deleteSectionId, setdeleteSectionId] = useState(null);
  const [isOpen, setIsOpen] = useState(false);
  const handleDeleteSection = async () => {
    await handleDelete(deleteSectionId);
    handleCloseDialog();
  };

  const handleCloseDialog = () => {
    setdeleteSectionId(null);
    setIsOpen(false);
  };

  const genOptions = Options;

  const [options, setOptions] = useState(genOptions || []);
  const customQuestion = QuestionDataTypeId === 0;

  function renderOptions(showOptions) {
    useEffect(() => {
      if (initialized) {
        handleSubmit();
      }
    }, [options]);

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
      setQuestion({ ...question, Options: options });
    };

    const onDragEnd = result => {
      const { source, destination } = result;
      if (!destination) return;

      const temp = [...options];
      const [sourceValue] = temp.splice(source.index, 1);
      temp.splice(destination.index, 0, sourceValue);
      setOptions(temp);
    };

    const renderOption = ({ option, index, dragHandleProps, errors, touched }) => {
      return (
        <Grid container className={classes.deleteIcon} key={`value-${index}`}>
          <Grid
            item
            lg={1}
            xs={1}
            sm={1}
            className={cs(classes.displayFlex, classes.optionsDragIcon)}
          >
            <Grid item lg={12} xs={12} sm={12} className={cs(classes.displayFlex)}>
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
            </Grid>
            <Grid item lg={0} xs={12} sm={12} className={cs(classes.displayFlex)}></Grid>
          </Grid>
          <Grid item container lg={10} xs={10} sm={10}>
            <Grid item lg={7} xs={12} sm={12} className={cs(classes.displayFlex)}>
              <TextField
                className={classes.formControlClass}
                size="small"
                fullWidth
                key={`${index}-${options.key}`}
                placeholder="Option"
                value={option?.key || ''}
                onChange={event => handleKeyChange(index, event)}
                helperText={touched?.key && errors?.key ? errors?.key : ''}
                error={touched?.key && errors?.key}
                disabled={!customQuestion || IsLocked}
              />
            </Grid>
            <Grid item lg={5} xs={12} sm={12}>
              <TextField
                className={classes.formControlClass}
                size="small"
                fullWidth
                key={`${index}-${options.value}`}
                placeholder="Value (Optional)"
                value={option?.value || ''}
                onChange={event => handleValueChange(index, event)}
                disabled={!customQuestion || IsLocked}
              />
            </Grid>
          </Grid>
          <Grid
            item
            lg={1}
            xs={1}
            sm={1}
            className={cs(classes.deleteIconHidden, customQuestion ? 'delete-icon-visible' : '')}
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
      showOptions && (
        <Grid container>
          <Grid lg={1}></Grid>
          <Grid lg={11}>
            <Box
              className={cs(classes.flexDirectionCol)}
              style={{ display: 'flex', padding: '8px 12px' }}
            >
              <DragDropContext onDragEnd={onDragEnd}>
                <Droppable key="list" droppableId="droppable-options-container" type="column">
                  {provided => (
                    <div ref={provided.innerRef} {...provided.droppableProps}>
                      {options.map(({ key, value }, index) => (
                        <Draggable
                          draggableId={`question-${ID}-option-${index}`}
                          index={toInteger(index)}
                          key={index}
                        >
                          {(provided, snapshot) => (
                            <Box
                              ref={provided.innerRef}
                              {...provided.draggableProps}
                              className={
                                snapshot.isDragging
                                  ? classes.whiteBorder
                                  : classes.transparentBorder
                              }
                            >
                              {renderOption({
                                option: { key, value },
                                index,
                                dragHandleProps: { ...provided.dragHandleProps },
                                errors: errors?.Options[index],
                                touched: touched?.Options[index]
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
              {touched?.Options && errors?.Options && typeof errors?.Options === 'string' && (
                <FormHelperText className={classes.maxWidth} error>
                  {errors?.Options}
                </FormHelperText>
              )}
              <Typography
                style={{
                  cursor: 'pointer',
                  textDecoration: 'underline',
                  padding: '12px 0px',
                  width: 'fit-content'
                }}
                onClick={() => {
                  handleAddNewOption();
                }}
              >
                Add new option
              </Typography>
            </Box>
          </Grid>
        </Grid>
      )
    );
  }
  const [timeoutConst, setTimeoutConst] = useState(null);

  const updateValue = () => {
    if (timeoutConst) {
      clearTimeout(timeoutConst);
    }
    const tempTimeout = setTimeout(() => {
      setFieldValue(index, question);
    }, 1500);
    setTimeoutConst(tempTimeout);
  };
  const maxLimit = QuestionTypeId === textBlock ? 4000 : 400;

  return (
    <Box className={classes.question}>
      <Grid container className={cs(classes.questionFieldsWrapper)}>
        <Grid item lg={12} sm={12} xs={12} container className={classes.displayFlex}>
          <Grid item lg={1} sm={1} xs={1} className={cs(classes.displayFlex)}>
            <span {...dragIndicatorProps}>
              <DragIndicatorIcon style={{ float: 'left' }} />
            </span>
            <Typography variant="h6">{index}</Typography>
          </Grid>
          <Grid item lg={8} sm={11} xs={11} className={classes.gridItemHeight}>
            <TextField
              className={classes.formControlClass}
              fullWidth
              size="small"
              variant="outlined"
              value={Text}
              helperText={
                touched?.Text && errors?.Text ? errors?.Text : `${Text?.length}/${maxLimit}`
              }
              error={touched?.Text && errors?.Text}
              inputProps={{
                maxLength: maxLimit
              }}
              multiline
              onChange={event => {
                setQuestion({ ...question, Text: event.target.value });
              }}
              disabled={!customQuestion || IsLocked}
            />
          </Grid>
          <Grid item lg={3} sm={3} xs={6} className={classes.gridItemHeight}>
            <DropDown
              question={{ ...question, ...{ options: questionTypesOptions } }}
              setFieldValue={key => setQuestion({ ...question, QuestionTypeId: key })}
              disabled={!customQuestion || IsLocked}
            />
          </Grid>
          <Grid lg={1}></Grid>
          <Grid
            item
            lg={11}
            sm={3}
            xs={6}
            className={cs(classes.displayFlex, classes.justifyContentSpaceBetween)}
          >
            {/* <Box className={classes.displayFlex}> */}
            <Grid item lg={2} sm={12} xs={12} className={classes.positionRelative}>
              <Button
                variant="outlined"
                className={classes.audienceButton}
                onClick={() => setShowAudience(!showAudience)}
                disabled={IsLocked}
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
            </Grid>
            {!isMobile && (
              <Grid item lg={10} sm={0} xs={0}>
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
              </Grid>
            )}
            {/* </Box> */}
          </Grid>
        </Grid>
        {renderOptions(showOptions)}
      </Grid>
      <Grid container className={classes.questionIconsWrapper}>
        <Box>
          <DeleteOutlineOutlinedIcon
            style={{
              color: `${customQuestion ? '#FF7070' : '#494949'}`,
              width: '30px',
              height: 'auto',
              margin: '4px 6px',
              cursor: `${customQuestion ? 'pointer' : 'not-allowed'}`
            }}
            onClick={async () => {
              if (customQuestion) {
                if (ID !== 0) {
                  setIsOpen(true);
                  setdeleteSectionId(index);
                } else {
                  await handleDelete(index, true);
                }
              }
            }}
          />
          <Dialog
            open={isOpen}
            onClose={handleCloseDialog}
            title={'Delete Question'}
            text={'Do you want to delete this question? This action is irreversible.'}
            onSubmit={handleDeleteSection}
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
        <Box>
          <HideCheckBox
            setFieldValue={isActive => setQuestion({ ...question, IsActive: isActive })}
            question={question}
            disabled={IsLocked}
          />
        </Box>
      </Grid>
    </Box>
  );
};

const HideCheckBox = ({ question, setFieldValue, disabled }) => {
  const { Text: title, QuestionDataTypeId, IsActive: value } = question;
  const unCheckedDisabled = value && disabled;
  const checkboxDisabled = unCheckedDisabled ? { color: '#ffffff !important' } : {};

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
    formControlLabelDisabled: {
      opacity: '0.5',
      color: 'black !important'
    },
    checked: {
      color: '#494949 !important',
      padding: 0,
      borderRadius: 3,
      background: 'transparent !important'
    },
    checkboxDisabled,
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
        disabled={disabled}
        control={
          <Checkbox
            data-testid={`${title}-${QuestionDataTypeId}-checkbox`}
            checked={!value}
            classes={{
              checked: classes.checked,
              root: classes.root,
              disabled: classes.checkboxDisabled
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
          root: classes.formControlLabelRoot,
          disabled: classes.formControlLabelDisabled
        }}
        onChange={() => {
          setFieldValue(!value);
        }}
      />
    </div>
  );
};

export default RenderQuestion;
