import { useDispatch } from 'react-redux';
import { createSlice } from '@reduxjs/toolkit';

import utils from 'utils';
import HeaderSlice from './HeaderSlice';
import schoolField from 'constants/schoolField';
import { responseMapper } from './helper';
import UpdateRegistrationRepository from 'repository/UpdateRegistrationRepository';

const UpdateRegistrationSlice = createSlice({
  name: 'updateRegistrationSlice',
  initialState: {
    msgs: [],
    school: {},
    activity: {},
    questions: [],
    loading: true,
    noSchool: false,
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
      const {
        RegistrantType,
        SchoolLocation,
        IsAgeRestricted,
        IsGradeRestricted,
        IsGenderRestricted
      } = activityDto;

      const { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer } = responseMapper(
        FormDto,
        RegistrantType,
        IsGradeRestricted,
        IsAgeRestricted,
        IsGenderRestricted
      );

      state.questions = [...formQuestionUpdated, schoolField(RegistrantType)];
      state.questionsGroups = [...formQuestionGroupUpdated];
      state.school = SchoolLocation || {};
      state.answeredQuestions = formQuestionAnswer;
      state.activity = activityDto;
    },
    submitFormSection: (state, { payload }) => {
      state.answeredQuestions = payload;
    },
    setRegistrationId: (state, { payload }) => {
      state.registrationId = payload;
    },
    setSchool: (state, { payload: { school, noSchool } }) => {
      state.school = school;
      state.noSchool = noSchool;
    },
    setMsgs: (state, { payload }) => {
      state.msgs = payload;
    }
  }
});

export const useUpdateRegistrationActions = () => {
  const {
    setMsgs,
    setSchool,
    setFormData,
    formDataLoaded,
    loadingFormData,
    submitFormSection,
    setRegistrationId
  } = UpdateRegistrationSlice.actions;

  const { setHeaderData } = HeaderSlice.actions;
  const dispatch = useDispatch();

  const loaded = () => dispatch(formDataLoaded());

  const loadFormData = async regUid => {
    dispatch(loadingFormData());
    await UpdateRegistrationRepository.getFormData(regUid).then(({ obj, msgs }) => {
      if (obj) dispatch(setFormData(obj));

      if (obj?.activityDto) {
        const { Name, Organization } = obj?.activityDto;
        dispatch(setHeaderData({ Name, Organization }));
      }
      dispatch(setMsgs(msgs));
      if (!msgs.length) dispatch(formDataLoaded());
    });
  };

  const submitFormSectionData = async (
    { answers, answerQuery, formData },
    lastSection,
    schoolLocationID,
    regUid
  ) => {
    const { answerQuery: answersQuery, groupId } = answerQuery;

    formData.append('groupId', groupId);
    formData.append('registrationUid', regUid);
    formData.append('schoolLocationID', schoolLocationID);
    formData.append('answers', new URLSearchParams(answersQuery).toString());

    await UpdateRegistrationRepository.submitFormSection(formData).then(({ data: { msgs } }) =>
      dispatch(setMsgs(msgs))
    );

    dispatch(submitFormSection(answers));
  };

  const setSchoolFieldValue = ({ school, noSchool }, submit) => {
    if (!submit) return dispatch(setSchool({ school, noSchool }));

    return UpdateRegistrationRepository.submitSchoolData(school).then(({ data: { obj, msgs } }) => {
      dispatch(setMsgs(msgs));
      dispatch(setSchool({ school: { ...school, id: obj }, noSchool }));
      return { obj, msgs };
    });
  };

  const clearMessages = () => {
    dispatch(setMsgs([]));
  };

  const exit = async (uid, activityUrl) => {
    await UpdateRegistrationRepository.exit(uid).then(({ msgs }) => {
      dispatch(setMsgs(msgs));
      const { redirect } = utils;
      if (!msgs.length) redirect(activityUrl);
    });
  };

  return { loadFormData, submitFormSectionData, setSchoolFieldValue, clearMessages, loaded, exit };
};

export default UpdateRegistrationSlice;
