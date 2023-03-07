import Validations from 'constants/regex';
import questionType from 'constants/questionType';

export default class RegistrationService {
  constructor(questions = [], currentQuestions = [], answeredQuestions = []) {
    this.questions = questions;
    this.answeredQuestions = answeredQuestions;
    this.currentQuestions = currentQuestions;
  }

  groupedQuestions = QuestionGroupDataTypeId => {
    return this.questions
      .filter(
        question => question.FormQuestionGroupQuestionGroupDataTypeId === QuestionGroupDataTypeId
      )
      .sort((q1, q2) => q1.DisplayOrder - q2.DisplayOrder);
  };

  initialValues = () => {
    const initialvalues = {};
    this.currentQuestions?.forEach(question => {
      initialvalues[question.ID] = this.submittedAnswer(question.QuestionDataTypeId, question.Text);
    });
    return initialvalues;
  };

  validator = values => {
    const { validateEmail, validatePhone } = Validations;
    const errors = {};
    const addToErrors = (ID, msg) => {
      errors[ID] = msg;
    };

    this.currentQuestions?.forEach(question => {
      const { ID, IsRequired, QuestionTypeId, Text } = question;
      const value = values[ID];

      if (IsRequired && QuestionTypeId === questionType.email && value) {
        if (!validateEmail(value)) addToErrors(ID, 'Please enter a valid Email address');
      } else if (IsRequired && !value) addToErrors(ID, `${Text} is required`);

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

    const getQuestionID = answer =>
      this.questions.find(q => q.QuestionDataTypeId === answer.QuestionDataTypeId)?.ID || 0;

    function addToAnswerQuery(ID, text) {
      answerQuery[`question-${ID}`] = text;
    }

    function composeAnswers(answer, ID) {
      if (!answer || answerCompiled.includes(answer.QuestionDataTypeId)) return;
      answers.push(answer);
      addToAnswerQuery(ID, answer.Text);
      answerCompiled.push(answer.QuestionDataTypeId);
    }

    keys.forEach(key => composeAnswers(this.answerObject(key, values), key));

    this.answeredQuestions.forEach(answer => composeAnswers(answer, getQuestionID(answer)));
    return { answers, answerQuery: { answerQuery, groupId: activeGroupId } };
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
      this.answeredQuestions.find(q => q.QuestionDataTypeId === id && q.QuestionText === text)
        ?.Text || '';
    return answer === 'null' ? '' : answer;
  };

  answerObject = (key, values) => {
    if (values[key] === null) return undefined;

    const question = this.questionObjectFindById(key);

    return {
      QuestionDataTypeId: question.QuestionDataTypeId,
      QuestionText: question.Text,
      Text: values[key]
    };
  };
  questionObjectFindById = id => this.currentQuestions.find(q => q.ID === Number(id)) || {};
}
