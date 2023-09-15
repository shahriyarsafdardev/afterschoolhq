import * as Yup from 'yup';
import { questionTypes as allQuestionTypes } from 'constants';

const formValidationSchema = values => {
  const { dropdown, radioList, checkBoxList, textBlock, mood } = allQuestionTypes;
  const isQuestionWithOptions = questionTypeId =>
    [dropdown, radioList, checkBoxList, mood].includes(questionTypeId);

  return Yup.object().shape(
    Object.keys(values).reduce((acc, key) => {
      acc[key] = Yup.object().shape({
        QuestionTypeId: Yup.number(),
        DisplayOrder: Yup.number(),
        Link: Yup.string()
          .nullable()
          .url(),
        Text: Yup.string().when(['QuestionTypeId'], {
          is: val => val !== textBlock,
          then: () =>
            Yup.string()
              .required('Text is Required')
              .max(400, 'Text cannot be longer than 400 characters'),
          otherwise: () =>
            Yup.string()
              .required('Text is Required')
              .max(4000, 'Text cannot be longer than 4000 characters')
        }),
        Options: Yup.array().when(['QuestionTypeId'], {
          is: val => isQuestionWithOptions(val),
          then: () =>
            Yup.array()
              .required('Options are required')
              .of(
                Yup.object().shape({
                  key: Yup.string().required('Text is required'),
                  value: Yup.string()
                })
              )
              .test(
                'options-length',
                'Atleast 2 options are required',
                options => options.length >= 2
              ),
          otherwise: () => Yup.array().notRequired()
        })
      });
      return acc;
    }, {})
  );
};
export default formValidationSchema;
