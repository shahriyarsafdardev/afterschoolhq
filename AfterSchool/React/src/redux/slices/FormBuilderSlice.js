import { useDispatch } from 'react-redux';
import { createSlice } from '@reduxjs/toolkit';

import { useErrorMessages } from 'redux/hooks';
import FormBuilderRepository from 'repository/FormBuilderRepository';

const FormBuilderSlice = createSlice({
  name: 'formBuilderSlice',
  initialState: {
    questions: [],
    form: {},
    loading: true,
    sectionLoading: true,
    sectionName: '',
    questionsGroups: []
  },
  reducers: {
    loadingFormData: state => {
      state.loading = true;
    },
    formDataLoaded: state => {
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
    updateForm: (state, { payload }) => {
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
    clearFormQuestions: state => {
      state.questions = [];
    }
  }
});

export const useFormBuilderActions = () => {
  const { setMsgs } = useErrorMessages();
  const {
    updateForm,
    setFormData,
    setSectionData,
    formDataLoaded,
    loadingFormData,
    sectionDataLoaded,
    clearFormQuestions,
    saveNewQuestionsGroup,
    loadingSectionData,
    updateQuestionsGroups
  } = FormBuilderSlice.actions;
  const dispatch = useDispatch();

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
        dispatch(updateForm(FormSettings));
        dispatch(formDataLoaded());
      }
    });
  };

  const saveNewSectionData = async values => {
    await FormBuilderRepository.saveNewSectionData(values).then(
      ({ data: { obj: newQuestionGroup, msgs } }) => {
        dispatch(saveNewQuestionsGroup(newQuestionGroup));
        dispatch(setMsgs(msgs));
      }
    );
  };

  const clearQuestions = () => {
    dispatch(clearFormQuestions());
  };

  return {
    loadFormData,
    clearQuestions,
    loadSectionData,
    deleteSectionData,
    updateFormSettings,
    saveNewSectionData,
    submitFormSectionData
  };
};

export default FormBuilderSlice;
