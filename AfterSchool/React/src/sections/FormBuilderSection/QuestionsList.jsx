import React from 'react';
import { Box } from '@material-ui/core';
import { DragDropContext, Droppable, Draggable } from 'react-beautiful-dnd';

import useStyles from './styles';
import RenderQuestion from './RenderQuestion';
import { cloneDeep } from 'lodash';

function QuestionsList(props) {
  const classes = useStyles();
  const { addNewQuestionHandler, setFieldValue, setReRender, setValues, reRender, form } = props;

  const { errors, values, touched } = props.formik;

  const onDragEnd = result => {
    const { source, destination } = result;
    if (!destination) return;

    const canNotDrop = !!values[destination.index].IsLocked;
    if (canNotDrop) return;

    moveObjectInFormikState(source.index, destination.index);
  };

  const moveObjectInFormikState = (source, destination) => {
    const sourceValue = cloneDeep(values[source]);
    const newState = cloneDeep(values);

    if (source < destination) {
      for (let i = source; i < destination; i++) {
        newState[i] = { ...newState[i + 1], DisplayOrder: Number(i) };
      }
    } else {
      for (let i = source; i > destination; i--) {
        newState[i] = { ...newState[i - 1], DisplayOrder: Number(i) };
      }
    }

    newState[destination] = { ...sourceValue, DisplayOrder: Number(destination) };

    setValues(newState, false);
    setReRender(!reRender);
  };

  const handleDelete = (key, removeFromList = false) => {
    const { [key]: deletedValue, ...restValues } = values;
    const newValues = {};
    Object.keys(restValues).forEach(k => {
      const newRow = Number(k) > Number(key) ? Number(k) - 1 : k;
      newValues[newRow] = { ...restValues[k], DisplayOrder: newRow };
    });

    if (!removeFromList) {
      const newKey = Object.keys(newValues).length + 1;
      newValues[newKey] = { ...deletedValue, IsDeleted: true, DisplayOrder: newKey };
    }

    setValues({ ...newValues }, false);
    setReRender(!reRender);
  };

  const handleCopy = key => {
    const copiedValue = cloneDeep(values[key]);

    const newValues = Object.keys(values).reduce((acc, k) => {
      const newKey = Number(k) > Number(key) ? Number(k) + 1 : k;
      acc[newKey] = cloneDeep(values[k]);
      acc[newKey].DisplayOrder = Number(newKey);
      return acc;
    }, {});

    newValues[Number(key) + 1] = {
      ...copiedValue,
      ID: 0,
      QuestionDataTypeId: 0,
      DisplayOrder: Number(key) + 1,
      IsLocked: false
    };

    setValues({ ...newValues }, false);
    setReRender(!reRender);
  };

  return (
    <Box>
      {values && (
        <DragDropContext onDragEnd={onDragEnd}>
          <Droppable key="list-2" droppableId="dropable-questions-container" type="column">
            {provided => (
              <div ref={provided.innerRef} {...provided.droppableProps}>
                {Object.keys(values).map(
                  key =>
                    !values[key].IsDeleted && (
                      <Draggable
                        isDragDisabled={values[key].IsLocked}
                        draggableId={`question-${key}`}
                        index={Number(key)}
                        key={key}
                      >
                        {(provided, snapshot) => (
                          <Box
                            ref={provided.innerRef}
                            {...provided.draggableProps}
                            className={
                              snapshot.isDragging ? classes.whiteBorder : classes.transparentBorder
                            }
                          >
                            <RenderQuestion
                              index={key}
                              key={`${key}-${values[key]?.ID}${reRender}`}
                              value={values[key]}
                              setFieldValue={setFieldValue}
                              handleDelete={handleDelete}
                              handleCopy={handleCopy}
                              handleAddNewQuestion={addNewQuestionHandler}
                              dragIndicatorProps={provided.dragHandleProps}
                              errors={errors[key]}
                              touched={touched[key]}
                              formType={form?.FormType}
                            />
                          </Box>
                        )}
                      </Draggable>
                    )
                )}
                {provided.placeholder}
              </div>
            )}
          </Droppable>
        </DragDropContext>
      )}
    </Box>
  );
}

export default QuestionsList;
