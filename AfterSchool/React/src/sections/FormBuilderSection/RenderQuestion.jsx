import cs from 'classnames';
import { styled } from '@material-ui/core/styles';
import React, { useEffect, useState } from 'react';
import {
  DragIndicator as DragIndicatorIcon,
  RadioButtonUnchecked as RadioButtonUncheckedIcon,
  CheckBoxOutlineBlank as CheckBoxOutlineBlankIcon,
  ArrowDropDownOutlined as ArrowDropDownOutlinedIcon
} from '@material-ui/icons';

import {
  FormControlLabel,
  FormHelperText,
  useMediaQuery,
  Typography,
  makeStyles,
  TextField,
  useTheme,
  Button,
  Chip,
  Grid,
  Box
} from '@material-ui/core';

import Dialog from './Dialog';
import useStyles from './styles';
import DropDown from './DropDown';
import AddIcon from 'assets/add.svg';
import CopyIcon from 'assets/copy.svg';
import DeleteIcon from 'assets/delete.svg';
import RenderCheckbox from './RenderCheckbox';
import RenderAudienceList from './RenderAudienceList';
import { audienceTypes, questionType } from 'constants';
import { DragDropContext, Droppable, Draggable } from 'react-beautiful-dnd';

const StyledImage = styled('img')(() => ({
  width: '20px',
  height: 'auto',
  margin: '4px 6px',
  cursor: 'pointer'
}));

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
  const [isOpen, setIsOpen] = useState(false);
  const [question, setQuestion] = useState(value);
  const [initialized, setInitialized] = useState(false);
  const [timeoutConst, setTimeoutConst] = useState(null);
  const [showAudience, setShowAudience] = useState(false);
  const [deleteSectionId, setdeleteSectionId] = useState(null);
  const { dropdown, radioList, checkBoxList, textBlock } = questionType;
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));
  const maxLimit = QuestionTypeId === textBlock ? 4000 : 400;
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
  const [options, setOptions] = useState(Options || []);
  const customQuestion = QuestionDataTypeId === 0;

  const updateValue = () => {
    if (timeoutConst) {
      clearTimeout(timeoutConst);
    }
    const tempTimeout = setTimeout(() => {
      setFieldValue(index, question);
    }, 1000);
    setTimeoutConst(tempTimeout);
  };
  const questionTypesOptions = Object.keys(questionType).map(key => ({
    key: questionType[key],
    value: key
  }));

  useEffect(() => {
    setInitialized(true);
  }, []);

  useEffect(() => {
    if (initialized) {
      updateValue();
    }
  }, [question]);

  const handleDeleteSection = async () => {
    await handleDelete(deleteSectionId);
    handleCloseDialog();
  };

  const handleCloseDialog = () => {
    setdeleteSectionId(null);
    setIsOpen(false);
  };

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
        <Grid container className={classes.deleteIcon} key={`value-${index}-${ID}`}>
          <Grid
            item
            lg={1}
            xs={1}
            sm={1}
            className={cs(classes.displayFlex, classes.optionsDragIcon)}
          >
            <Grid item lg={12} xs={12} sm={12} className={cs(classes.displayFlex)}>
              <span {...dragHandleProps} data-testid={`dragIconForOptions-${ID}-${index}`}>
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
                key={`${ID}-${index}-${QuestionDataTypeId}`}
                placeholder="Option"
                value={option?.key || ''}
                onChange={event => handleKeyChange(index, event)}
                helperText={touched?.key && errors?.key ? errors?.key : ''}
                error={touched?.key && errors?.key}
                disabled={!customQuestion || IsLocked}
                data-testid={`keyOfOption-${ID}-${index}`}
              />
            </Grid>
            <Grid item lg={5} xs={12} sm={12}>
              <TextField
                className={classes.formControlClass}
                size="small"
                fullWidth
                key={`${ID}-${index}-${QuestionDataTypeId}`}
                placeholder="Value (Optional)"
                value={option?.value || ''}
                onChange={event => handleValueChange(index, event)}
                disabled={!customQuestion || IsLocked}
                data-testid={`valueOfOption-${ID}-${index}`}
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
            <StyledImage
              src={DeleteIcon}
              style={{
                width: '18px'
              }}
              onClick={() => handleDeleteOption(index)}
              data-testid={`deleteIconOfOption-${ID}-${index}`}
            />
          </Grid>
        </Grid>
      );
    };

    return (
      showOptions && (
        <Grid item lg={12} md={12} sm={12} xs={12} container>
          <Grid lg={1}></Grid>
          <Grid lg={11} md={12} sm={12} xs={12}>
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
                          isDragDisabled={!customQuestion}
                          index={Number(index)}
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
              {!(!customQuestion || IsLocked) && (
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
              )}
            </Box>
          </Grid>
        </Grid>
      )
    );
  }

  return (
    <Box className={classes.question}>
      <Grid container className={cs(classes.questionFieldsWrapper)}>
        <Grid item lg={12} sm={12} xs={12} container className={classes.displayFlex}>
          <Grid item lg={1} sm={1} xs={1} className={cs(classes.displayFlex)}>
            <span {...dragIndicatorProps} data-testid={`dragIconForQuestions-${ID}-${index}`}>
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
              data-testid={`questionNameField-${ID}-${index}`}
            />
          </Grid>
          <Grid item lg={3} md={4} sm={4} xs={6} className={classes.gridItemHeight}>
            <DropDown
              question={{ ...question, ...{ options: questionTypesOptions } }}
              setFieldValue={key => setQuestion({ ...question, QuestionTypeId: key })}
              disabled={!customQuestion || IsLocked}
              dataTestId={`questionTypeDropDown-${ID}-${index}`}
            />
          </Grid>
          <Grid lg={1}></Grid>
          <Grid item lg={11} md={8} sm={6} xs={6} className={cs(classes.audienceWrapper)}>
            <Grid item lg={2} md={5} sm={12} xs={12} className={classes.positionRelative}>
              <Button
                variant="outlined"
                className={classes.audienceButton}
                onClick={() => setShowAudience(!showAudience)}
                disabled={IsLocked}
                data-testid={`audienceButtonForQuestion-${ID}-${index}`}
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
                  dataTestId={`audienceListForQuestion-${ID}-${index}`}
                />
              )}
            </Grid>
            {!isMobile && (
              <Grid
                item
                lg={9}
                md={6}
                sm={0}
                xs={0}
                className={cs(classes.displayFlex, classes.flexWrap)}
              >
                {QuestionSettings.map(({ Audience, Show, Required }) => {
                  if (!(Show || Required)) return;

                  return (
                    <Chip
                      style={{ backgroundColor: '#F2F2F5', margin: '1px' }}
                      label={`${audienceTypes[Audience]}${Required ? '*' : ''}`}
                      key={Audience}
                    />
                  );
                })}
              </Grid>
            )}
          </Grid>
        </Grid>
        {renderOptions(showOptions)}
      </Grid>
      <Grid container className={classes.questionIconsWrapper}>
        <Box className={classes.displayFlex}>
          <StyledImage
            src={DeleteIcon}
            style={{
              opacity: `${customQuestion ? '1' : '0.5'}`,
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
            data-testid={`deleteIconOfQuestion-${ID}-${index}`}
          />
          <Dialog
            open={isOpen}
            onClose={handleCloseDialog}
            title={'Delete Question'}
            text={'Do you want to delete this question? This action is irreversible.'}
            onSubmit={handleDeleteSection}
            dataTestId={`deleteQuestion-${ID}-${index}`}
          />
          <StyledImage
            src={CopyIcon}
            style={{
              opacity: `${!IsLocked ? '1' : '0.5'}`,
              cursor: `${!IsLocked ? 'pointer' : 'not-allowed'}`,
              width: '30px'
            }}
            data-testid={`copyIconOfQuestion-${ID}-${index}`}
            onClick={() => {
              if (!IsLocked) handleCopy(index);
            }}
          />
          <StyledImage
            src={AddIcon}
            style={{
              opacity: `${!IsLocked ? '1' : '0.5'}`,
              cursor: `${!IsLocked ? 'pointer' : 'not-allowed'}`,
              width: '25px'
            }}
            onClick={() => {
              if (!IsLocked) handleAddNewQuestion(index);
            }}
            data-testid={`addQuestionBelowIcon-${ID}-${index}`}
          />
        </Box>
        <Box>
          <HideCheckBox
            setFieldValue={isActive => setQuestion({ ...question, IsActive: isActive })}
            dataTestId={`hideCheckboxOfQuestion-${ID}-${index}`}
            question={question}
            disabled={IsLocked}
          />
        </Box>
      </Grid>
    </Box>
  );
};

const HideCheckBox = ({ question, setFieldValue, disabled, dataTestId }) => {
  const { IsActive: value } = question;
  const unCheckedDisabled = value && disabled;

  const classes = makeStyles(() => ({
    checkboxWrapper: {
      padding: '0px 6px'
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
    }
  }))();

  return (
    <Box className={classes.checkboxWrapper}>
      <FormControlLabel
        data-testid={`${dataTestId}-form`}
        disabled={disabled}
        control={
          <RenderCheckbox
            checked={!value}
            handleCheck={() => setFieldValue(!value)}
            dataTestId={`${dataTestId}-checkbox`}
            unCheckedDisabled={unCheckedDisabled}
            disabled={disabled}
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
    </Box>
  );
};

export default RenderQuestion;
