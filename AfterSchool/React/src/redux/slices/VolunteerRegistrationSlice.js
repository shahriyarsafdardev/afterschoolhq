import utils from 'utils';
import Routes from 'routes';
import HeaderSlice from './HeaderSlice';
import { useDispatch } from 'react-redux';
import { responseMapper } from './helper';
import { createSlice } from '@reduxjs/toolkit';
import ConfirmRegistrationSection from 'constants/eSignatureSection';
import VolunteerRegistrationRepository from 'repository/VolunteerRegistrationRepository';

const VolunteerRegistrationSlice = createSlice({
  name: 'volunteerRegistrationSlice',
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
      const { RegistrationIsCompleted, RegistrationUid, CanAccess } = activityDto;
      const { volunteerThankYouUrl, volunteerAccountUrl } = Routes;
      const { redirect } = utils;

      if (!CanAccess) return redirect(volunteerAccountUrl);
      else if (RegistrationIsCompleted) return redirect(volunteerThankYouUrl(RegistrationUid));

      const { questions, questionsGroups } = ConfirmRegistrationSection();
      const { formQuestionUpdated, formQuestionGroupUpdated, formQuestionAnswer } = responseMapper(
        FormDto,
        activityDto.RegistrantType,
        activityDto.IsGradeRestricted,
        activityDto.IsAgeRestricted,
        activityDto.IsGenderRestricted
      );

      state.questions = [...questions, ...formQuestionUpdated];
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
    setMsgs: (state, { payload }) => {
      state.msgs = payload;
    }
  }
});

export const useVolunteerRegistrationActions = () => {
  const {
    setMsgs,
    setFormData,
    formDataLoaded,
    loadingFormData,
    submitFormSection,
    setRegistrationId
  } = VolunteerRegistrationSlice.actions;
  const { setHeaderData, loadingCompleted: headerLoadingCompleted } = HeaderSlice.actions;
  const dispatch = useDispatch();

  const loaded = () => dispatch(formDataLoaded());

  const loadFormData = async (accountName, activityName, regUid) => {
    dispatch(loadingFormData());
    await VolunteerRegistrationRepository.getFormData(accountName, activityName, regUid).then(
      ({ obj, msgs }) => {
        if (obj) dispatch(setFormData(obj));

        dispatch(setMsgs(msgs));
        const {
          Name,
          CanAccess,
          Organization,
          HeaderImageURL,
          RegistrationIsCompleted
        } = obj?.activityDto;

        if (CanAccess && !RegistrationIsCompleted && !msgs.length) {
          dispatch(setHeaderData({ HeaderImageURL, Name, Organization }));
          dispatch(headerLoadingCompleted());
          dispatch(formDataLoaded());
        }
      }
    );
  };

  const submitFormSectionData = async (
    { answers, answerQuery, formData },
    lastSection,
    accountName,
    activityName,
    regUid
  ) => {
    const { answerQuery: answersQuery, groupId } = answerQuery;

    formData.append('registrantUid', regUid);
    formData.append('accountName', accountName);
    formData.append('activityName', activityName);
    formData.append('groupId', groupId);
    formData.append('answers', new URLSearchParams(answersQuery).toString());

    if (lastSection) {
      const eSignature = answers.find(answer => answer.QuestionText === 'E-Signature').Text;
      formData.append('eSignature', eSignature);
      dispatch(loadingFormData());
      VolunteerRegistrationRepository.register(formData).then(({ data: { obj, msgs } }) => {
        dispatch(setRegistrationId(obj));
        dispatch(setMsgs(msgs));
      });
    } else
      await VolunteerRegistrationRepository.submitFormSection(formData).then(({ data: { msgs } }) =>
        dispatch(setMsgs(msgs))
      );

    dispatch(submitFormSection(answers));
  };

  const clearMessages = () => {
    dispatch(setMsgs([]));
  };

  const exit = async (uid, activityUrl) => {
    await VolunteerRegistrationRepository.exit(uid).then(({ msgs }) => {
      dispatch(setMsgs(msgs));
      const { redirect } = utils;
      if (!msgs.length) redirect(activityUrl);
    });
  };

  return { loadFormData, submitFormSectionData, clearMessages, loaded, exit };
};

export default VolunteerRegistrationSlice;
