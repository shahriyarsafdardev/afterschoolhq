import Validations from 'constants/regex';
import questionType from 'constants/questionType';

export default class RegistrationService {
  constructor(questions = [], answeredQuestions = [], currentQuestions = []) {
    this.questions = questions;
    this.answeredQuestions = answeredQuestions;
    this.currentQuestions = currentQuestions;
  }

  groupedQuestions = QuestionGroupDataTypeId => {
    return this.questions
      .filter(
        question => question.FormQuestionGroupQuestionGroupDataTypeId === QuestionGroupDataTypeId
      )
      .sort((q1, q2) => q1.DispalayOrder - q2.DispalayOrder);
  };

  initialValues = () => {
    const initialvalues = {};
    this.currentQuestions?.forEach(question => {
      initialvalues[question.ID] = this.submittedAnswer(question.QuestionDataTypeId, question.Text);
    });
    return initialvalues;
  };

  validator = values => {
    const { emailValidationRegex, phoneValidationRegex } = Validations;
    const errors = {};
    this.currentQuestions?.forEach(question => {
      const { ID, IsRequired, QuestionTypeId, Text } = question;
      const valueOfCurrentQuestion = values[ID];
      if (IsRequired && QuestionTypeId === questionType.email && valueOfCurrentQuestion) {
        if (!emailValidationRegex.test(valueOfCurrentQuestion)) {
          errors[ID] = 'Please enter a valid email address';
        }
      } else if (IsRequired && !valueOfCurrentQuestion) {
        errors[ID] = `${Text} is required`;
      }

      if (valueOfCurrentQuestion) {
        if (QuestionTypeId === questionType.email) {
          if (!emailValidationRegex.test(valueOfCurrentQuestion)) {
            errors[ID] = 'Please enter a valid email address';
          }
        }
        if (QuestionTypeId === questionType.text && Text.includes('Phone')) {
          if (!phoneValidationRegex.test(valueOfCurrentQuestion)) {
            errors[ID] = 'Please enter a valid phone number';
          }
        }
      }
    });
    return errors;
  };

  formSectionSubmitObject = (values, activeGroupId) => {
    const answerQuery = {};
    const answers = [];
    const keys = Object.keys(values);
    const getQuestionID = answer =>
      this.questions.find(q => q.QuestionDataTypeId === answer.QuestionDataTypeId)?.ID || 0;
    function addToAnswerQuery(ID, text) {
      answerQuery[`question-${ID}`] = text;
    }
    function composeAnswers(answer, ID, shouldCompose) {
      if (!shouldCompose) return;
      answers.push(answer);
      addToAnswerQuery(ID, answer.Text);
    }
    this.answeredQuestions.forEach(answer => {
      composeAnswers(answer, getQuestionID(answer), !keys.includes(answer.QuestionDataTypeId));
    });
    keys.forEach(key => {
      const answer = this.answerObject(key, values);
      composeAnswers(answer, key, answer);
    });
    return { answers, answerQuery: { answerQuery, groupId: activeGroupId } };
  };

  //Private methods, only being called within the class
  submittedAnswer = (id, text) => {
    return (
      this.answeredQuestions.find(q => q.QuestionDataTypeId === id && q.QuestionText === text)
        ?.Text || ''
    );
  };
  answerObject = (key, values) => {
    if (!values[key]) return undefined;
    const question = this.questionObjectFindById(key);
    return {
      QuestionDataTypeId: question.QuestionDataTypeId,
      QuestionText: question.Text,
      Text: values[key]
    };
  };
  questionObjectFindById = id => {
    return this.currentQuestions.find(q => q.ID === Number(id)) || {};
  };
}
