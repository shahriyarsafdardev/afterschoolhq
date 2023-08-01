export { simpleQuestionTypes as questionType } from './questionType';
export const restrictedQuestionsDataTypeIds = { dateOfBirth: 4, grade: 44, gender: 45 };

export const gradeIds = {
  'Pre-K': 0,
  K: 1,
  '1st': 2,
  '2nd': 3,
  '3rd': 4,
  '4th': 5,
  '5th': 6,
  '6th': 7,
  '7th': 8,
  '8th': 9,
  '9th': 10,
  '10th': 11,
  '11th': 12,
  '12th': 13
};

export const audienceTypes = ['All', 'Parent', 'Student', 'Adult'];
export const formTypes = { 0: 'Not Set', 1: 'StudentForm', 2: 'Survey', 3: 'VolunteerForm' };

export const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB in bytes
export const validFileSize = file => file && file.size <= MAX_FILE_SIZE;

export const eSignMsg = allowToPayLater =>
  allowToPayLater
    ? 'Your registration is not finalized until you have selected a payment option.'
    : ' Your registration is not finalized until you complete payment.';
export const applyModes = { Apply: 'Apply', Register: 'Register' };
export const googlePlaces = ['places'];
