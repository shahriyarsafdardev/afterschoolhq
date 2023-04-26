import React from 'react';
import questionType from 'constants/questionType';

import {
  FileUpload,
  DateQuestion,
  RadioQuestion,
  TextAreaQuestion,
  DropdownQuestion
} from 'components/Questions';
import {
  CheckBox,
  TextBlock,
  TextField,
  SchoolField,
  CheckBoxList,
  ApiTextField
} from 'components';

function ShowQuestion({ currentQuestion, formik, currentQuestions, isUpdating }) {
  const { QuestionTypeId } = currentQuestion;

  switch (QuestionTypeId) {
    case questionType.text:
      return 'School' === currentQuestion.Text ? (
        <SchoolField question={currentQuestion} formik={formik} isUpdating={isUpdating} />
      ) : 'Street Address' === currentQuestion.Text ? (
        <ApiTextField
          question={currentQuestion}
          formik={formik}
          currentQuestions={currentQuestions}
        />
      ) : (
        <TextField question={currentQuestion} formik={formik} />
      );
    case questionType.email:
      return <TextField question={currentQuestion} formik={formik} />;
    case questionType.date:
      return <DateQuestion question={currentQuestion} formik={formik} />;
    case questionType.dropdown:
      return <DropdownQuestion question={currentQuestion} formik={formik} />;
    case questionType.radioList:
      return <RadioQuestion question={currentQuestion} formik={formik} />;
    case questionType.checkBoxList:
      return <CheckBoxList question={currentQuestion} formik={formik} />;
    case questionType.textArea:
      return <TextAreaQuestion question={currentQuestion} formik={formik} />;
    case questionType.checkBox:
      return <CheckBox question={currentQuestion} formik={formik} />;
    case questionType.fileUpload:
      return <FileUpload question={currentQuestion} formik={formik} />;
    case questionType.textBlock:
      return <TextBlock question={currentQuestion} />;
    default:
      return <TextField question={currentQuestion} formik={formik} />;
  }
}

export default ShowQuestion;
