import React from 'react';
import questionType from 'constants/questionType';

import {
  FileUpload,
  MoodQuestion,
  DateQuestion,
  RadioQuestion,
  AudioQuestion,
  VideoQuestion,
  PhotoQuestion,
  RatingQuestion,
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

function ShowQuestion({
  index,
  formik,
  isUpdating,
  currentQuestion,
  currentQuestions,
  isUpdatingRegistration
}) {
  const { QuestionTypeId } = currentQuestion;

  switch (QuestionTypeId) {
    case questionType.text:
      return 'School' === currentQuestion.Text ? (
        <SchoolField
          question={currentQuestion}
          formik={formik}
          isUpdating={isUpdating}
          key={index}
          isUpdatingRegistration={isUpdatingRegistration}
        />
      ) : 'Street Address' === currentQuestion.Text ? (
        <ApiTextField
          question={currentQuestion}
          formik={formik}
          currentQuestions={currentQuestions}
          key={index}
        />
      ) : (
        <TextField question={currentQuestion} formik={formik} key={index} />
      );
    case questionType.email:
      return <TextField question={currentQuestion} formik={formik} key={index} />;
    case questionType.number:
      return <TextField question={currentQuestion} formik={formik} type={'number'} key={index} />;
    case questionType.date:
      return <DateQuestion question={currentQuestion} formik={formik} key={index} />;
    case questionType.dropdown:
      return <DropdownQuestion question={currentQuestion} formik={formik} key={index} />;
    case questionType.radioList:
      return <RadioQuestion question={currentQuestion} formik={formik} key={index} />;
    case questionType.checkBoxList:
      return <CheckBoxList question={currentQuestion} formik={formik} key={index} />;
    case questionType.textArea:
      return <TextAreaQuestion question={currentQuestion} formik={formik} key={index} />;
    case questionType.checkBox:
      return <CheckBox question={currentQuestion} formik={formik} key={index} />;
    case questionType.fileUpload:
      return <FileUpload question={currentQuestion} formik={formik} key={index} />;
    case questionType.audio:
      return <AudioQuestion question={currentQuestion} formik={formik} key={index} />;
    case questionType.video:
      return <VideoQuestion question={currentQuestion} formik={formik} key={index} />;
    case questionType.rating:
      return <RatingQuestion question={currentQuestion} formik={formik} key={index} />;
    case questionType.photo:
      return <PhotoQuestion question={currentQuestion} formik={formik} key={index} />;
    case questionType.mood:
      return <MoodQuestion question={currentQuestion} formik={formik} key={index} />;
    case questionType.textBlock:
      return <TextBlock question={currentQuestion} key={index} />;
    default:
      return <TextField question={currentQuestion} formik={formik} key={index} />;
  }
}

export default ShowQuestion;
