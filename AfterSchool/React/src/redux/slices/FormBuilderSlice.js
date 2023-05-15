import { useDispatch } from 'react-redux';
import { createSlice } from '@reduxjs/toolkit';

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
    saveNewQuestionsGroup: (state, { payload }) => {
      state.questionsGroups = [...state.questionsGroups, payload];
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
    saveNewQuestionsGroup,
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

  const submitFormSectionData = async ({ FormQuestionGroups, ...values }) => {
    dispatch(loadingFormData());
    await FormBuilderRepository.submitFormSection(values).then(({ data: { msgs } }) => {
      dispatch(setMsgs(msgs));
      if (!msgs.length) {
        dispatch(setSectionData(values));
        dispatch(updateQuestionsGroups(FormQuestionGroups));
        dispatch(formDataLoaded());
      }
    });
  };

  const updateFormSettings = async ({ FormSettings }) => {
    dispatch(loadingFormData());
    await FormBuilderRepository.submitFormSection({ FormSettings }).then(({ data: { msgs } }) => {
      dispatch(setMsgs(msgs));
      if (!msgs.length) {
        dispatch(updateform(FormSettings));
        dispatch(formDataLoaded());
      }
    });
  };

  const saveNewSectionData = async values => {
    // dispatch(loadingFormData());
    await FormBuilderRepository.saveNewSectionData(values).then(
      ({ data: { obj: newQuestionGroup, msgs } }) => {
        dispatch(saveNewQuestionsGroup(newQuestionGroup));
        dispatch(setMsgs(msgs));
        //   if (!msgs.length) dispatch(formDataLoaded());
      }
    );
  };
  const updateQuestionsGroupsData = questionGroups => {
    dispatch(updateQuestionsGroups(questionGroups));
  };

  const setCurrentSectionName = sectionName => {
    dispatch(setSectionName(sectionName));
  };

  return {
    loadFormData,
    loadSectionData,
    updateFormSettings,
    setCurrentSectionName,
    submitFormSectionData,
    updateQuestionsGroupsData,
    saveNewSectionData,
    loaded
  };
};

export default FormBuilderSlice;
