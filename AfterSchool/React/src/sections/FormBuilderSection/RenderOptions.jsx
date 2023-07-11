import React, { useEffect } from 'react';
import cs from 'classnames';

import {
  DragIndicator as DragIndicatorIcon,
  RadioButtonUnchecked as RadioButtonUncheckedIcon,
  CheckBoxOutlineBlank as CheckBoxOutlineBlankIcon
} from '@material-ui/icons';

import { FormHelperText, Typography, TextField, Grid, Box } from '@material-ui/core';

import useStyles from './styles';
import DeleteIcon from 'assets/delete.svg';
import { StyledImage } from './helper';
import { questionType } from 'constants';
import { DragDropContext, Droppable, Draggable } from 'react-beautiful-dnd';

function RenderOptions(props) {
  const classes = useStyles();

  const {
    errors,
    touched,
    options,
    question,
    setOptions,
    showOptions,
    initialized,
    setQuestion
  } = props;

  const { ID, IsLocked, QuestionTypeId, QuestionDataTypeId } = question;
  const { radioList, checkBoxList } = questionType;
  const customQuestion = QuestionDataTypeId === 0;
  const radioListIcon = radioList === QuestionTypeId;
  const checkboxListIcon = checkBoxList === QuestionTypeId;
  const optionsWrapperClass = `${classes.optionsComponent} ${
    showOptions ? classes.showOptionsComponent : ''
  }`;

  useEffect(() => {
    if (initialized && options.length > 0) handleSubmit();
  }, [options]);

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
              <DragIndicatorIcon style={{ float: 'left', color: '#B2B2B4' }} />
            </span>
            {checkboxListIcon ? (
              <CheckBoxOutlineBlankIcon />
            ) : radioListIcon ? (
              <RadioButtonUncheckedIcon />
            ) : (
              <Typography variant="body1">{index + 1}.</Typography>
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
    <Box className={optionsWrapperClass}>
      {showOptions && (
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
                          {provided => (
                            <Box ref={provided.innerRef} {...provided.draggableProps}>
                              {renderOption({
                                index,
                                option: { key, value },
                                dragHandleProps: { ...provided.dragHandleProps },
                                errors: errors?.Options && errors?.Options[index],
                                touched: touched?.Options && touched?.Options[index]
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
                <FormHelperText
                  data-testid={`${ID}-${QuestionDataTypeId}-error`}
                  className={classes.maxWidth}
                  error
                >
                  {errors?.Options}
                </FormHelperText>
              )}
              {!(!customQuestion || IsLocked) && (
                <Typography className={classes.adddNewOption} onClick={handleAddNewOption}>
                  Add new option
                </Typography>
              )}
            </Box>
          </Grid>
        </Grid>
      )}
    </Box>
  );
}

export default RenderOptions;
