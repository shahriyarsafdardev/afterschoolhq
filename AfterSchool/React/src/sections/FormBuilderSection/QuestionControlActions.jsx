import React from 'react';
import Dialog from './Dialog';
import AddIcon from 'assets/add.svg';
import CopyIcon from 'assets/copy.svg';
import DeleteIcon from 'assets/delete.svg';
import { Box } from '@material-ui/core';
import useStyles from './styles';
import { StyledImage, deleteItemDialogProps } from './helper';

function QuestionControlActions(props) {
  const classes = useStyles();

  const {
    index,
    isOpen,
    question,
    setIsOpen,
    handleCopy,
    handleDelete,
    handleCloseDialog,
    setdeleteSectionId,
    handleDeleteSection,
    handleAddNewQuestion
  } = props;

  const { ID, IsLocked, QuestionDataTypeId } = question;
  const customQuestion = QuestionDataTypeId === 0;

  const handleDeleteQuestion = async () => {
    if (customQuestion) {
      if (ID !== 0) {
        setIsOpen(true);
        setdeleteSectionId(index);
      } else {
        await handleDelete(index, true);
      }
    }
  };

  return (
    <Box className={classes.displayFlex}>
      <StyledImage
        src={DeleteIcon}
        style={{
          opacity: `${customQuestion ? '1' : '0.5'}`,
          cursor: `${customQuestion ? 'pointer' : 'not-allowed'}`
        }}
        onClick={() => handleDeleteQuestion()}
        data-testid={`deleteIconOfQuestion-${ID}-${index}`}
      />
      <Dialog
        open={isOpen}
        onClose={handleCloseDialog}
        onSubmit={handleDeleteSection}
        {...deleteItemDialogProps('Question')}
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
  );
}

export default QuestionControlActions;
