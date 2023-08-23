export { simpleQuestionTypes as questionType } from './questionType';
export const restrictedQuestionsDataTypeIds = { dateOfBirth: 4, grade: 44, gender: 45 };

export const PER_PAGE = 3;
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

export const DEFAULT_QUESTION_SETTINGS = [
  { Audience: 1, Show: true, Required: true },
  { Audience: 2, Show: true, Required: true },
  { Audience: 3, Show: true, Required: true }
];

export const audienceTypes = ['All', 'Parent', 'Student', 'Adult'];
export const formTypes = { 0: 'Not Set', 1: 'StudentForm', 2: 'Survey', 3: 'VolunteerForm' };

export const isFileTypeValid = (file, validExtensions) => {
  const fileExtension = file.type.split('/')[1];
  return validExtensions.includes(fileExtension) || validExtensions.includes(file.type);
};

export const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB in bytes
export const validFileSize = file => file && file.size <= MAX_FILE_SIZE;

export const isValidFile = (file, validExtensions) => {
  const validFile = isFileTypeValid(file, validExtensions);
  if (!validFile) return 'File Type is not valid';

  const validSize = validFileSize(file);
  if (!validSize) return 'File size cannot be greater than 5MB';
  return;
};

export const eSignMsg = allowToPayLater =>
  allowToPayLater
    ? 'Your registration is not finalized until you have selected a payment option.'
    : ' Your registration is not finalized until you complete payment.';
export const applyModes = { Apply: 'Apply', Register: 'Register' };
export const googleLocationAPI = `https://maps.googleapis.com/maps/api/js?key=${process.env.GOOGLE_MAPS_API_KEY}&libraries=places`;

export const changeDateTimeFormat = dateTime => {
  return new Date(dateTime).toLocaleString('en-US', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: 'numeric',
    minute: 'numeric',
    hour12: true
  });
};

export const getAndFormatDateTime = (now = new Date()) => {
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  const hours = String(now.getHours()).padStart(2, '0');
  const minutes = String(now.getMinutes()).padStart(2, '0');

  return `${year}-${month}-${day}T${hours}:${minutes}`;
};
