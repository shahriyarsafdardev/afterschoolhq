import { createSlice } from '@reduxjs/toolkit';
import { useDispatch } from 'react-redux';
import RegistrationRepository from 'repository/RegistrationRepository';
import ConfirmRegistrationSection from 'constants/eSignatureSection';
import schoolField from 'constants/schoolField';

const RegistrationSlice = createSlice({
  name: 'registrationSlice',
  initialState: {
    msgs: [],
    activity: {},
    questions: [],
    loading: true,
    questionsGroups: [],
    registrationId: null,
    answeredQuestions: []
  },
  reducers: {
    loadingFormData: state => {
      state.loading = true;
    },
    formDataLoaded: state => {
      state.loading = false;
    },
    formDataLoadingFailed: state => {
      state.loading = false;
    },
    setFormData: (state, { payload: { FormDto, activityDto } }) => {
      const { questions, questionsGroups } = ConfirmRegistrationSection();
      const { formQuestion, formQuestionGroup, formQuestionAnswer } = FormDto;

      state.questions = [...formQuestion, ...questions, schoolField(activityDto.RegistrantType)];
      state.questionsGroups = [...formQuestionGroup, ...questionsGroups];
      state.answeredQuestions = formQuestionAnswer;
      state.activity = activityDto;
    },
    submitFormSection: (state, { payload }) => {
      state.answeredQuestions = payload;
    },
    setRegistrationId: (state, { payload }) => {
      state.registrationId = payload;
    },
    setMsgs: (state, { payload }) => {
      state.msgs = payload;
    }
  }
});

export const useRegistrationActions = () => {
  const {
    formDataLoaded,
    loadingFormData,
    setFormData,
    setMsgs,
    setRegistrationId,
    submitFormSection
  } = RegistrationSlice.actions;
  const dispatch = useDispatch();

  const loadFormData = async () => {
    dispatch(loadingFormData());
    const { obj, msgs } = await RegistrationRepository.getFormData();
    if (obj) dispatch(setFormData(obj));
    dispatch(setMsgs(msgs));
    dispatch(formDataLoaded());
  };

  const submitFormSectionData = async ({ answers, answerQuery }, lastSection) => {
    const { answerQuery: answersQuery, groupId } = answerQuery;
    const params = { answers: new URLSearchParams(answersQuery).toString(), groupId };

    if (lastSection) {
      const eSignature = answers.find(answer => answer.QuestionText === 'E-Signature').Text;
      dispatch(loadingFormData());
      RegistrationRepository.register({ ...params, eSignature }).then(({ data: { obj, msgs } }) => {
        dispatch(setRegistrationId(obj));
        dispatch(setMsgs(msgs));
        dispatch(formDataLoaded());
      });
    } else
      await RegistrationRepository.submitFormSection(params).then(({ data: { msgs } }) =>
        dispatch(setMsgs(msgs))
      );

    dispatch(submitFormSection(answers));
  };

  return { loadFormData, submitFormSectionData };
};

export default RegistrationSlice;
