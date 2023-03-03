import { useDispatch } from 'react-redux';
import { responseMapper } from './helper';
import { createSlice } from '@reduxjs/toolkit';
import schoolField from 'constants/schoolField';
import ConfirmRegistrationSection from 'constants/eSignatureSection';
import RegistrationRepository from 'repository/RegistrationRepository';

const RegistrationSlice = createSlice({
  name: 'registrationSlice',
  initialState: {
    msgs: [],
    school: {},
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
      const { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer } = responseMapper(
        FormDto
      );

      state.questions = [
        ...questions,
        ...formQuestionUpdated,
        schoolField(activityDto.RegistrantType)
      ];
      state.questionsGroups = [...formQuestionGroupUpdated, ...questionsGroups];
      state.answeredQuestions = formQuestionAnswer;
      state.activity = activityDto;
    },
    submitFormSection: (state, { payload }) => {
      state.answeredQuestions = payload;
    },
    setRegistrationId: (state, { payload }) => {
      state.registrationId = payload;
    },
    setSchool: (state, { payload }) => {
      state.school = payload;
    },
    setMsgs: (state, { payload }) => {
      state.msgs = payload;
    }
  }
});

export const useRegistrationActions = () => {
  const {
    setMsgs,
    setSchool,
    setFormData,
    formDataLoaded,
    loadingFormData,
    submitFormSection,
    setRegistrationId
  } = RegistrationSlice.actions;
  const dispatch = useDispatch();

  const loadFormData = async (newChild = '0') => {
    dispatch(loadingFormData());
    await RegistrationRepository.getFormData(newChild).then(({ obj, msgs }) => {
      if (obj) dispatch(setFormData(obj));
      dispatch(setMsgs(msgs));
      if (!msgs.length) dispatch(formDataLoaded());
    });
  };

  const submitFormSectionData = async (
    { answers, answerQuery },
    lastSection,
    schoolLocationID,
    newChild
  ) => {
    const { answerQuery: answersQuery, groupId } = answerQuery;
    const params = {
      newChild,
      groupId,
      schoolLocationID,
      answers: new URLSearchParams(answersQuery).toString()
    };

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

  const setSchoolFieldValue = (school, submit) => {
    if (!submit) return dispatch(setSchool(school));

    return RegistrationRepository.submitSchoolData(school).then(({ data: { obj, msgs } }) => {
      dispatch(setMsgs(msgs));
      dispatch(setSchool({ ...school, id: obj }));
      return { obj, msgs };
    });
  };

  return { loadFormData, submitFormSectionData, setSchoolFieldValue };
};

export default RegistrationSlice;
