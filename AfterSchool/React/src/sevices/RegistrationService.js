import { calculateAge } from './helper';
import Validations from 'constants/regex';
import questionType from 'constants/questionType';
import { gradeIds, restrictedQuestionsDataTypeIds } from 'constants';
import { isEqual } from 'lodash';

export default class RegistrationService {
  constructor(questions = [], currentQuestions = [], answeredQuestions = [], isSurvey = false) {
    this.isSurvey = isSurvey;
    this.questions = questions;
    this.answeredQuestions = answeredQuestions;
    this.currentQuestions = currentQuestions;
  }

  groupedQuestions = (QuestionGroupDataTypeId, GroupTitle) => {
    return this.questions
      .filter(
        question =>
          question.FormQuestionGroupQuestionGroupDataTypeId === QuestionGroupDataTypeId &&
          question.GroupTitle === GroupTitle
      )
      .sort((q1, q2) => q1.DisplayOrder - q2.DisplayOrder);
  };

  initialValues = () => {
    const initialvalues = {};
    this.currentQuestions?.forEach(question => {
      initialvalues[question.ID] = this.isSurvey
        ? this.surveySubmittedAnswer(question.ID, this.isFile(question.QuestionTypeId, true))
        : this.submittedAnswer(question.QuestionDataTypeId, question.Text);
    });
    return initialvalues;
  };

  validator = (values, genderRestrictions = {}, ageRestrictions = {}, gradeRestrictions = {}) => {
    const { validateEmail, validatePhone } = Validations;
    const errors = {};
    const addToErrors = (ID, msg) => {
      errors[ID] = msg;
    };

    this.currentQuestions?.forEach(question => {
      const { ID, IsRequired, QuestionTypeId, QuestionDataTypeId, Text, Options } = question;
      const value = values[ID];
      const restrictionParams = [QuestionDataTypeId, value, addToErrors, ID];

      this.checkGenderRestriction(...restrictionParams, genderRestrictions);
      this.checkAgeRestriction(...restrictionParams, ageRestrictions);
      this.checkGradeRestriction(...restrictionParams, gradeRestrictions, Options);

      if (IsRequired && QuestionTypeId === questionType.email && value) {
        if (!validateEmail(value)) addToErrors(ID, 'Please enter a valid Email address');
      } else if (IsRequired && !value) addToErrors(ID, `${Text} is required`);

      if (IsRequired && this.isFile(question.QuestionTypeId, true)) {
        isEqual(value, { Text: '', Description: undefined }) &&
          addToErrors(ID, `${Text} is required`);
      }

      if (!value) return;

      if (QuestionTypeId === questionType.email) {
        if (!validateEmail(value)) addToErrors(ID, 'Please enter a valid email address');
      } else if (QuestionTypeId === questionType.text && Text.includes('Phone')) {
        if (!validatePhone(value)) addToErrors(ID, 'Please enter a valid phone number');
      }
    });
    return errors;
  };

  formSectionSubmitObject = (values, activeGroupId) => {
    const filteredValues = Object.keys(values).filter(key => key !== 99999); //SchoolField value is removed from query string as it is being sent separately
    const [answerQuery, answers, keys, answerCompiled] = [{}, [], filteredValues, []];

    const formData = new FormData();
    const getQuestionID = answer =>
      this.questions.find(
        q => q.QuestionDataTypeId === answer.QuestionDataTypeId && q.Text === answer.QuestionText
      )?.ID;

    function addToAnswerQuery(ID, text) {
      answerQuery[`question-${ID}`] = text;
    }
    function addInFiles(ID, answer) {
      formData.append(`question-${ID}`, answer.Text);
    }

    const composeAnswers = (answer, ID) => {
      const alreadyAnswered = answerCompiled.find(
        ans =>
          ans.QuestionDataTypeId === answer.QuestionDataTypeId &&
          ans.QuestionText === answer.QuestionText
      );

      if (answer && !alreadyAnswered) {
        if (this.isFile(answer.QuestionTypeId, false)) {
          if (this.isFile(answer.QuestionTypeId, true)) {
            addInFiles(ID, answer);
            addToAnswerQuery(ID, answer?.Description || '');
            answers.push({ ...answer, Text: { name: answer?.Text } });
          } else {
            addInFiles(ID, answer);
            answers.push({ ...answer, Text: { name: answer.Text.name } });
          }
        } else {
          addToAnswerQuery(ID, answer.Text);
          answers.push(answer);
        }
        answerCompiled.push(answer);
      }
    };

    keys.forEach(key => composeAnswers(this.answerObject(key, values), key));

    this.answeredQuestions?.forEach(answer => composeAnswers(answer, getQuestionID(answer)));
    return { answers, answerQuery: { answerQuery, groupId: activeGroupId }, formData };
  };

  saveButtonText = (lastSection, RegistrantType, IsFree, skipAble) => {
    let buttonText = '';
    if (skipAble) {
      buttonText = 'Skip';
    } else if (lastSection) {
      if (RegistrantType === 'Parent') {
        if (IsFree) {
          buttonText = 'Confirm Registration';
        } else {
          buttonText = 'E-Sign and Continue to Payment';
        }
      } else if (RegistrantType === 'Volunteer') {
        buttonText = 'Complete Registration';
      } else {
        buttonText = 'Confirm';
      }
    } else {
      buttonText = 'Save & Continue';
    }
    return buttonText;
  };

  //Private methods, only being called within the class
  submittedAnswer = (id, text) => {
    const answer =
      this.answeredQuestions?.find(q => q.QuestionDataTypeId === id && q.QuestionText === text)
        ?.Text || '';
    return answer === 'null' ? '' : answer;
  };

  surveySubmittedAnswer = (id, isFile) => {
    const answer = this.answeredQuestions?.find(q => q.FormQuestionId === id);
    const text = answer?.Text === 'null' ? '' : answer?.Text || '';
    if (!isFile) return text;

    return { Text: text, Description: answer?.Description };
  };

  isFile = (type, withDescription) => {
    const { fileUpload, audio, video, photo } = questionType;
    if (withDescription) return [audio, video, photo].includes(type);

    return [fileUpload, audio, video, photo].includes(type);
  };

  answerObject = (key, values) => {
    if (values[key] === null) return undefined;

    const question = this.questionObjectFindById(key);

    const answer = {
      QuestionDataTypeId: question.QuestionDataTypeId,
      QuestionTypeId: question.QuestionTypeId,
      QuestionText: question.Text,
      Text: values[key]
    };

    if (this.isFile(question.QuestionTypeId, true))
      return { ...answer, Text: values[key]?.File, Description: values[key].Description };

    return answer;
  };
  questionObjectFindById = id => this.currentQuestions.find(q => q.ID === Number(id)) || {};

  checkAgeRestriction(questionDataTypeId, value, addToErrors, id, ageRestrictions) {
    const { isAgeRestricted = false, minAge, maxAge } = ageRestrictions;

    if (!isAgeRestricted || questionDataTypeId !== restrictedQuestionsDataTypeIds.dateOfBirth) {
      return;
    }

    const age = calculateAge(value);
    const error = `Age should be between ${minAge} and ${maxAge}`;

    if (age < minAge) {
      addToErrors(id, error);
    } else if (age > maxAge) {
      addToErrors(id, error);
    }
  }

  checkGradeRestriction(questionDataTypeId, value, addToErrors, id, gradeRestrictions, options) {
    const { maxGrade, minGrade, isGradeRestricted = false } = gradeRestrictions;

    if (questionDataTypeId !== restrictedQuestionsDataTypeIds.grade || !isGradeRestricted) {
      return;
    }

    const grade = value ? JSON.parse(value)[0].value : '';
    const optionGrades = options.split(',').map(option => gradeIds[option]);
    const validOptions = optionGrades.filter(
      optionGrade => optionGrade >= gradeIds[minGrade] && optionGrade <= gradeIds[maxGrade]
    );

    if (grade && grade !== 'null' && !validOptions.includes(gradeIds[grade])) {
      addToErrors(id, `Grade should be between ${minGrade} and ${maxGrade}`);
    }
  }

  checkGenderRestriction(questionDataTypeId, value, addToErrors, id, genderRestrictions) {
    const { enforcedGender = 0, isGenderRestricted = false } = genderRestrictions;

    if (
      questionDataTypeId !== restrictedQuestionsDataTypeIds.gender ||
      !isGenderRestricted ||
      enforcedGender === 0
    ) {
      return;
    }

    const gender = value ? JSON.parse(value)[0].value : '';
    const validGender = enforcedGender === 1 ? 'Male' : 'Female';

    if (gender !== validGender) {
      addToErrors(id, `Gender should be ${validGender} only`);
    }
  }
}
