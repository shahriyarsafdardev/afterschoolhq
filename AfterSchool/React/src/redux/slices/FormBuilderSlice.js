import { useDispatch } from 'react-redux';
import { createSlice } from '@reduxjs/toolkit';

import utils from 'utils';
import schoolField from 'constants/schoolField';
import { responseMapper, handleRedirection } from './helper';
import ConfirmRegistrationSection from 'constants/eSignatureSection';
import FormBuilderRepository from 'repository/FormBuilderRepository';

const FormBuilderSlice = createSlice({
  name: 'formBuilderSlice',
  initialState: {
    msgs: [],
    questions: [],
    form: {},
    loading: true,
    sectionLoading: true,
    sectionName: '',
    questionsGroups: [],
    registrationId: null
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
    setFormData: (state, { payload: { Form, FormQuestionGroup } }) => {
      state.form = Form;
      state.questionsGroups = FormQuestionGroup;
    },
    updateQuestionsGroups: (state, { payload }) => {
      state.questionsGroups = payload;
    },
    updateform: (state, { payload }) => {
      state.form = payload;
    },
    loadingSectionData: state => {
      state.sectionLoading = true;
    },
    sectionDataLoaded: state => {
      state.sectionLoading = false;
    },
    setSectionData: (state, { payload: { FormQuestion } }) => {
      state.questions = FormQuestion;
    },
    submitFormSection: (state, { payload }) => {
      state.answeredQuestions = payload;
    },
    setRegistrationId: (state, { payload }) => {
      state.registrationId = payload;
    },
    setSectionName: (state, { payload }) => {
      state.sectionName = payload;
    },
    setMsgs: (state, { payload }) => {
      state.msgs = payload;
    }
  }
});

export const useFormBuilderActions = () => {
  const {
    setMsgs,
    updateform,
    setFormData,
    setSectionData,
    formDataLoaded,
    setSectionName,
    loadingFormData,
    sectionDataLoaded,
    submitFormSection,
    setRegistrationId,
    loadingSectionData,
    updateQuestionsGroups
  } = FormBuilderSlice.actions;
  const dispatch = useDispatch();

  const loaded = () => dispatch(formDataLoaded());

  const loadFormData = async formId => {
    dispatch(loadingFormData());
    await FormBuilderRepository.getFormData(formId).then(({ obj, msgs }) => {
      if (obj) dispatch(setFormData(obj));
      dispatch(setMsgs(msgs));
      if (!msgs.length) dispatch(formDataLoaded());
    });
  };

  const loadSectionData = async (formId, sectionId) => {
    dispatch(loadingSectionData());
    await FormBuilderRepository.getSectionData(formId, sectionId).then(({ obj, msgs }) => {
      if (obj) dispatch(setSectionData(obj));
      dispatch(setMsgs(msgs));
      if (!msgs.length) dispatch(sectionDataLoaded());
    });
  };

  const submitFormSectionData = async values => {
    dispatch(loadingFormData());
    await FormBuilderRepository.submitFormSection(values).then(({ data: { msgs } }) => {
      dispatch(setMsgs(msgs));
      if (!msgs.length) dispatch(formDataLoaded());
    });
  };

  const updateFormSettings = async ({ FormSettings }) => {
    dispatch(updateform(FormSettings));
    await submitFormSectionData({ FormSettings });
  };

  const updateQuestionsGroupsData = questionGroups => {
    dispatch(updateQuestionsGroups(questionGroups));
  };

  //   const setSchoolFieldValue = ({ school, noSchool }, submit) => {
  //     if (!submit) return dispatch(setSchool({ school, noSchool }));

  //     return FormBuilderRepository.submitSchoolData(school).then(({ data: { obj, msgs } }) => {
  //       dispatch(setMsgs(msgs));
  //       dispatch(setSchool({ school: { ...school, id: obj }, noSchool }));
  //       return { obj, msgs };
  //     });
  //   };

  //   const clearMessages = () => {
  //     dispatch(setMsgs([]));
  //   };

  const setCurrentSectionName = sectionName => {
    dispatch(setSectionName(sectionName));
  };

  //   const exit = async (uid, activityUrl) => {
  //     await FormBuilderRepository.exit(uid).then(({ msgs }) => {
  //       dispatch(setMsgs(msgs));
  //       const { redirect } = utils;
  //       if (!msgs.length) redirect(activityUrl);
  //     });
  //   };

  return {
    loadFormData,
    loadSectionData,
    updateFormSettings,
    setCurrentSectionName,
    submitFormSectionData,
    updateQuestionsGroupsData,
    //  submitFormSectionData, setSchoolFieldValue, clearMessages,
    loaded
    //  exit
  };
};

export default FormBuilderSlice;
