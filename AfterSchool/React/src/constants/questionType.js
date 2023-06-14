export const simpleQuestionTypes = {
  date: 8,
  text: 1,
  email: 7,
  number: 9,
  textArea: 2,
  dropdown: 6,
  checkBox: 5,
  radioList: 3,
  textBlock: 11,
  fileUpload: 12,
  checkBoxList: 4
};

const questionTypes = {
  ...simpleQuestionTypes,
  video: 13,
  audio: 14,
  rating: 17,
  mood: 16,
  photo: 15
};

export default questionTypes;
