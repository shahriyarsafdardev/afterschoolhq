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

export const MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB in bytes
export const validFileSize = file => file && file.size <= MAX_FILE_SIZE;
