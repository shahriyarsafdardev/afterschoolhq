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
    },
    clearFormQuestions: state => {
      state.questions = [];
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
    clearFormQuestions,
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
    await FormBuilderRepository.submitFormSection({ ...values, SaveQuestion: true }).then(
      ({
        data: {
          msgs,
          obj: { FormQuestion }
        }
      }) => {
        dispatch(setMsgs(msgs));
        if (!msgs.length) {
          dispatch(setSectionData({ ...values, FormQuestion }));
          dispatch(updateQuestionsGroups(FormQuestionGroups));
        }
      }
    );
  };

  const deleteSectionData = async ({ FormQuestionGroups, ...values }) => {
    await FormBuilderRepository.submitFormSection(values).then(({ data: { msgs } }) => {
      dispatch(setMsgs(msgs));
      if (!msgs.length) {
        dispatch(updateQuestionsGroups(FormQuestionGroups));
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

  const clearQuestions = () => {
    dispatch(clearFormQuestions());
  };

  return {
    loadFormData,
    loadSectionData,
    deleteSectionData,
    updateFormSettings,
    setCurrentSectionName,
    submitFormSectionData,
    updateQuestionsGroupsData,
    saveNewSectionData,
    clearQuestions,
    loaded
  };
};

export default FormBuilderSlice;
