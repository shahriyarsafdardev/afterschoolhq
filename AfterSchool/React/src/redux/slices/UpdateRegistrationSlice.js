import { useDispatch } from 'react-redux';
import { createSlice } from '@reduxjs/toolkit';

import utils from 'utils';
import HeaderSlice from './HeaderSlice';
import schoolField from 'constants/schoolField';
import { responseMapper } from './helper';
import ErrorMessagesSlice from './ErrorMessagesSlice';
import ConfirmRegistrationSection from 'constants/eSignatureSection';
import UpdateRegistrationRepository from 'repository/UpdateRegistrationRepository';

const UpdateRegistrationSlice = createSlice({
  name: 'updateRegistrationSlice',
  initialState: {
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

      const { questions, questionsGroups } = ConfirmRegistrationSection();
      const { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer } = responseMapper(
        FormDto,
        RegistrantType,
        IsGradeRestricted,
        IsAgeRestricted,
        IsGenderRestricted
      );

      state.questions = [...questions, ...formQuestionUpdated, schoolField(RegistrantType)];
      state.questionsGroups = [...formQuestionGroupUpdated, ...questionsGroups];
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
    }
  }
});

export const useUpdateRegistrationActions = () => {
  const { setMsgs } = ErrorMessagesSlice.actions;
  const {
    setSchool,
    setFormData,
    formDataLoaded,
    loadingFormData,
    submitFormSection
  } = UpdateRegistrationSlice.actions;

  const { setHeaderData, loadingCompleted: headerLoadingCompleted } = HeaderSlice.actions;
  const dispatch = useDispatch();

  const loaded = () => dispatch(formDataLoaded());

  const loadFormData = async regUid => {
    dispatch(loadingFormData());
    await UpdateRegistrationRepository.getFormData(regUid).then(({ obj, msgs }) => {
      if (obj) dispatch(setFormData(obj));

      if (obj?.activityDto) {
        const { HeaderImageURL, Name, Organization } = obj?.activityDto;
        dispatch(setHeaderData({ HeaderImageURL, Name, Organization }));
        dispatch(headerLoadingCompleted());
      }
      if (!msgs.length) dispatch(formDataLoaded());
    });
  };

  const submitFormSectionData = async (
    { answers, answerQuery, formData },
    lastSection,
    schoolLocationID,
    regUid
  ) => {
    dispatch(submitFormSection(answers));
    const { answerQuery: answersQuery, groupId } = answerQuery;

    formData.append('groupId', groupId);
    formData.append('registrationUid', regUid);
    formData.append('schoolLocationID', schoolLocationID);
    formData.append('answers', new URLSearchParams(answersQuery).toString());

    if (lastSection) {
      const eSignature = answers.find(answer => answer.QuestionText === 'E-Signature').Text;
      formData.append('eSignature', eSignature);
      dispatch(loadingFormData());
      return await UpdateRegistrationRepository.eSignature(formData).then(
        ({ data: { msgs } }) => msgs
      );
    } else
      return await UpdateRegistrationRepository.submitFormSection(formData).then(
        ({ data: { msgs } }) => msgs
      );
  };

  const setSchoolFieldValue = ({ school, noSchool }, submit) => {
    if (!submit) return dispatch(setSchool({ school, noSchool }));

    return UpdateRegistrationRepository.submitSchoolData(school).then(({ data: { obj, msgs } }) => {
      dispatch(setSchool({ school: { ...school, id: obj }, noSchool }));
      return { obj, msgs };
    });
  };

  const clearMessages = () => {
    dispatch(setMsgs([]));
  };

  const exit = async (uid, activityUrl) => {
    await UpdateRegistrationRepository.exit(uid).then(({ msgs }) => {
      const { redirect } = utils;
      if (!msgs.length) redirect(activityUrl);
    });
  };

  return { loadFormData, submitFormSectionData, setSchoolFieldValue, clearMessages, loaded, exit };
};

export default UpdateRegistrationSlice;
