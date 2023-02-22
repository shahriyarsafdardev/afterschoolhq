using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Controllers
{
    public static class RegistrationCommon
    {
        public static ParticipationObject GetParticipationObjectFromCart()
        {
            var participationObjects = SessionVariables.Cart?.GetCartItems().Select(cart => cart.GetParticipationObject()).ToList();
            ParticipationObject participationObject = null;

            participationObjects?.ForEach(item =>
            {
                if (item.IsActivityGroup())
                    participationObject = item;
            });

            return participationObject ?? participationObjects?.FirstOrDefault();
        }

        public static List<FormQuestion> GetAllDefaultPersonalInformationQuestions(SystemMessages msgs)
        {
            var administratorDefaultForm = FormService.GetAdministratorAccountStudentForm(msgs);
            var personalContactGroup = GetQuestionGroups(msgs, administratorDefaultForm)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            return GetAllQuestionsForQuestionGroup(msgs, personalContactGroup);
        }

        public static List<FormQuestion> GetAllDefaultVolunteerPersonalInformationQuestions(SystemMessages msgs)
        {
            var administratorDefaultForm = FormService.GetAdministratorAccountVolunteerForm(msgs);
            var personalContactGroup = GetQuestionGroups(msgs, administratorDefaultForm)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            return GetAllQuestionsForQuestionGroup(msgs, personalContactGroup);
        }

        private static List<FormQuestion> GetAllQuestionsForQuestionGroup(SystemMessages msgs,
            FormQuestionGroup questionGroup)
        {
            return questionGroup.GetAllQuestions(msgs);
        }

        private static List<FormQuestionGroup> GetQuestionGroups(SystemMessages msgs, Form form)
        {
            return form.GetQuestionGroups(msgs);
        }

        public static WebUser CreateOrUpdateUser(WebUser currentUser, string firstName, string lastName, string email,
            string phone, string password, bool allowTextMessages, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (currentUser.IsEmpty())
            {
                return WebUserService.CreateNew(
                    firstName,
                    lastName,
                    email.Trim(),
                    phone.Trim(),
                    password,
                    true,
                    true,
                    false,
                    allowTextMessages,
                    null,
                    msgs,
                    dc);
            }

            var user = SessionVariables.CurrentUser;
            user.Phone = phone.Trim();
            user.AllowTextMessages = allowTextMessages;
            user.LastModifiedBy = currentUser.ID;
            user.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(user.Save(dc));
            return user;
        }

        public static WebUser CreateOrUpdateUserWithOptionalPhone(WebUser userToUpdate, string firstName, string lastName, string email,
            string phone, string password, bool allowTextMessages, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (userToUpdate.IsEmpty())
                return WebUserService.CreateNewWithOptionalPhone(
                    firstName,
                    lastName,
                    email.Trim(),
                    phone.IfEmpty("").Trim(),
                    password,
                    true,
                    true,
                    false,
                    allowTextMessages,
                    null,
                    msgs,
                    dc);

            var user = userToUpdate;
            user.Phone = phone.IfEmpty("").Trim();
            user.AllowTextMessages = allowTextMessages;
            user.LastModifiedBy = userToUpdate.ID;
            user.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(user.Save(dc));
            return user;
        }

        public static Account GetOrCreateParentOrStudentRegistrantAccount(WebUser user, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var account = user.GetRegistrantAccount(msgs, dc);

            // create account
            if (account.IsEmpty())
                account = user.AddNewParentOrStudentRegistrantAccount(
                    UserType.Owner,
                    true,
                    user,
                    msgs,
                    dc);

            return account;
        }

        public static Account GetOrCreateVolunteerAccount(WebUser user, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var account = user.GetVolunteerAccount(msgs, dc);

            // create account
            if (account.IsEmpty())
                account = user.AddNewVolunteerAccount(
                    UserType.Owner,
                    true,
                    user,
                    msgs,
                    dc);

            return account;
        }
        public static Registrant GetOrCreateParentOrGuardianRegistrant(Account account, WebUser user, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            return GetOrCreateParentOrGuardianRegistrant(account, user, false, msgs, dc);
        }

        public static Registrant GetOrCreateParentOrGuardianRegistrant(Account account, WebUser user, bool allowTextMessages, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            var registrant = account.GetFirstParentOrGuardianRegistrant(msgs, dc);

            if (registrant.IsEmpty())
                // create parent registrant record
                registrant = account.CreateParentOrStudentRegistrant(
                    user,
                    user.AsList(),
                    RegistrantType.Individual,
                    true,
                    true,
                    allowTextMessages,
                    null,
                    RegistrantProfileStatus.Started,
                    user,
                    msgs,
                    dc);

            return registrant;
        }

        public static Registrant GetOrCreateVolunteerRegistrant(Account account, WebUser user, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            var registrant = account.GetVolunteerRegistrant(msgs, dc);

            if (registrant.IsEmpty())
                // create parent registrant record
                registrant = account.CreateVolunteerRegistrant(
                    user,
                    user.AsList(),
                    true,
                    false,
                    RegistrantProfileStatus.Started,
                    user,
                    msgs,
                    dc);

            return registrant;
        }

        public static void AddOrUpdateStandardAnswers(List<FormQuestionViewModel> qvms, Registrant currentRegistrant,
            WebUser currentUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            qvms.Where(qvm => qvm.Question.QuestionDataTypeId != QuestionDataType.NotSet)
                .ToList()
                .ForEach(qvm =>
                {
                    currentRegistrant.AddOrUpdateAnswer(
                        qvm.Value,
                        qvm.Question,
                        currentUser,
                        msgs,
                        dc);
                });
        }

        public static List<FormQuestionViewModel> GetQuestionsRequiredForActivityThatHaveNotYetBeenCollected(ParticipationObject participationObject,
            Registrant currentRegistrant,
            SystemMessages msgs)
        {
            var allQuestions = participationObject.Form.GetActiveQuestions(msgs).ToList();
            var answers = currentRegistrant.GetAnswers(msgs);

            var requiredQuestionsThatNeedAnswers = allQuestions
                .Where(question => question.IsRequired &&
                                   question.FormQuestionGroupRegistrantTypeId == RegistrantType.Individual &&
                                   question.QuestionDataTypeId != QuestionDataType.NotSet)
                .Where(question => answers.FirstOrDefault(answer => answer.QuestionDataTypeId == question.QuestionDataTypeId).IsEmpty() ||
                                   answers.First(answer => answer.QuestionDataTypeId == question.QuestionDataTypeId).Text.IsBlank())
                .Distinct()
                .ToList();

            var customQuestionsThatNeedAnswers = allQuestions
                .Where(question => !question.IsDeleted && question.QuestionDataTypeId == QuestionDataType.NotSet)
                .Where(question => answers.Find(answer => answer.FormQuestionId == question.ID).IsEmpty())
                .OrderBy(question => question.DisplayOrder)
                .ToList();

            var questionsThatNeedAnswers = requiredQuestionsThatNeedAnswers.Concat(customQuestionsThatNeedAnswers).ToList();
            return questionsThatNeedAnswers.Select(question => question.ToFormQuestionViewModel()).ToList();
        }

        public static List<DateTime> GetSelectedDatesFromCart()
        {
            return SessionVariables.Cart?.GetCartItems().FirstOrDefault()?.SelectedDates?.Split(',')
                .Select(x => x.ToDateTime()).ToList();
        }

        public static List<FormQuestion> GetActiveCustomQuestionsThatHaveNotYetBeenAnswered(ParticipationObject participationObject, SystemMessages msgs)
        {
            var allQuestions = participationObject.Form.GetActiveQuestions(msgs).ToList();
            var answers = SessionVariables.CurrentRegistrant.GetAnswers(msgs);
            var questions = allQuestions
                .Where(question => !question.IsDeleted
                                   && question.QuestionDataTypeId == QuestionDataType.NotSet
                                   && answers.Find(answer => answer.FormQuestionId == question.ID).IsEmpty())
                .OrderBy(question => question.DisplayOrder)
                .ToList();

            return questions;
        }

        public static void ValidateRegisterStudentQuestions(List<FormQuestion> questions, SystemMessages msgs, HttpRequestBase httpRequestBase)
        {
            questions.ForEach(question =>
            {
                if (!question.IsRequired) return;

                var populated = true;

                if (question.QuestionTypeId == QuestionType.FileUpload &&
                    httpRequestBase.Files[question.ToFormQuestionViewModel().Name].IsEmpty())
                    populated = false;
                else if (question.QuestionTypeId != QuestionType.FileUpload &&
                         httpRequestBase[question.ToFormQuestionViewModel().Name].IsEmpty())
                    populated = false;

                if (!populated)
                    msgs.Add($"{question.Text} is required", MessageType.UserError);
            });
        }

        public static Registration CreateRegistration(ParticipationObject participationObject,
            List<FormQuestionViewModel> qvms, string signature, HttpRequestBase request, SystemMessages msgs = null)
        {
            Registration registration;
            Registrant submittedRecord;
            var currentAccount = SessionVariables.CurrentAccount;
            var currentRegistrant = SessionVariables.CurrentRegistrant;
            var currentUser = SessionVariables.CurrentUser;

            (submittedRecord, registration) = currentRegistrant.GetSubmittedRecord(participationObject, msgs);

            if (msgs.HasNoErrors())
                Database.ExecuteTransaction
                (
                    (amsgs, adc) =>
                    {
                        var period = participationObject.GetCurrentRegistrationPeriod(amsgs, adc);

                        if (qvms.IsNotEmpty())
                            AddOrUpdateStandardAnswers(
                                qvms,
                                currentRegistrant,
                                currentUser,
                                amsgs,
                                adc);

                        SubmitRecord(
                            ref registration,
                            ref submittedRecord,
                            currentRegistrant,
                            participationObject,
                            currentUser,
                            period,
                            signature,
                            request.UserAgent, 
                            request.GetUserHostAddress(),
                            amsgs,
                            adc);

                        if (qvms.IsNotEmpty())
                            AddCustomQuestions(
                                qvms,
                                submittedRecord,
                                registration,
                                currentUser,
                                request,
                                amsgs, adc);

                        if (amsgs.HasNoErrors() && participationObject.IsSession())
                            AddSessionActivitiesToRegistration(
                                participationObject,
                                registration,
                                submittedRecord,
                                period,
                                request,
                                amsgs, adc);
                    },
                    smsgs => msgs.AddRange(smsgs),
                    fmsgs => msgs.AddRange(fmsgs),
                    emsgs => Utilities.SystemErrorLog(emsgs)
                );

            if (msgs.HasNoErrors())
                // LOG: Someone Registered RegistrantName for ParticipationObjectType ParticipationObjectName
                SessionVariables.WebSession.LogActivity(
                    currentAccount,
                    currentUser,
                    WebSessionActionType.Registered,
                    ObjectType.Registrant,
                    objectId: currentRegistrant.ID.Value,
                    objectText: currentRegistrant.FullName,
                    indirectObjectType: participationObject.GetWebSessionObjectType(),
                    indirectObjectId: participationObject.ObjectID.Value,
                    indirectObjectText: participationObject.Name
                );

            return registration;
        }

        public static void SubmitRecord(ref Registration registration, ref Registrant submittedRecord,
            Registrant currentRegistrant, ParticipationObject participationObject, WebUser currentUser,
            RegistrationPeriod period, string signature, string userAgent, string hostAddress, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // account gets copy / snapshot of master registrant record
            // required in order to make sure registration data doesn't change as registrant changes information in master record.
            if (submittedRecord.IsEmpty())
            {
                submittedRecord = currentRegistrant.SubmitRecordToAccount(
                    participationObject.OwnerAccount,
                    currentUser,
                    msgs,
                    dc);

                // create or update registration
                registration = submittedRecord.AddRegistrationWithESignature(
                    participationObject.OwnerAccount.ID.Value,
                    participationObject.GetAnyRelatedActivityGroupId(),
                    participationObject.GetAccountActivityGroupId(),
                    period?.ID,
                    signature,
                    userAgent.MaxChars(400), 
                    hostAddress,
                    currentUser.ID.Value,
                    msgs,
                    dc);
            }
            else
            {
                registration.AddESignature(
                    signature,
                    userAgent.MaxChars(400), 
                    hostAddress,
                    currentUser,
                    msgs,
                    dc);
            }

            //copy all answers from the main account to the registrant & registration :)
            currentRegistrant.CopyAnswersTo(
                submittedRecord,
                registration,
                participationObject.Form,
                currentUser,
                msgs,
                dc);
        }

        public static void AddCustomQuestions(List<FormQuestionViewModel> qvms, Registrant submittedRecord,
            Registration registration, WebUser currentUser, HttpRequestBase request, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // add custom answers to questions directly to this registration
            qvms.Where(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.NotSet)
                .ToList()
                .ForEach(qvm =>
                {
                    if (qvm.Question.QuestionTypeId == QuestionType.FileUpload)
                    {
                        if (request.Files[qvm.Name].IsNotEmpty() && request.Files[qvm.Name].ContentLength > 0)
                        {
                            var currentFileSizeInBytes = request.Files[qvm.Name].ContentLength;
                            //5 MB = 5242880 Bytes
                            //File uploaded here should be less than of 5MB
                            if (currentFileSizeInBytes <= 5242880)
                                submittedRecord.AddOrUpdateAnswer(
                                    request.Files[qvm.Name].InputStream,
                                    request.Files[qvm.Name].FileName,
                                    qvm.Question,
                                    registration.ID,
                                    currentUser,
                                    msgs,
                                    dc);
                            else
                                msgs.Add("Upload a file less than or equal to 5 megabytes", MessageType.UserError);
                        }
                    }
                    else
                    {
                        submittedRecord.AddOrUpdateAnswer(
                            qvm.Value,
                            qvm.Question,
                            registration?.ID,
                            currentUser,
                            msgs,
                            dc);
                    }
                });
        }

        public static void AddSessionActivitiesToRegistration(ParticipationObject participationObject, Registration registration, Registrant submittedRecord,
            RegistrationPeriod period, HttpRequestBase request, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // if there is a Session, then a person may set their preferences for activities in that session, 
            // also only if the session is set to Apply
            if (participationObject.IsSession())
            {
                // only get activities that are in the cart
                var participationObjects = GetAllParticipationObjectsForCheckout(registration, msgs, dc);

                if (participationObject.RegistrationMode == RegistrationMode.Apply)
                {
                    for (var i = 1; i <= participationObjects.Count; i++)
                    {
                        var postedValue = request[$"activity-preference-{i}"];
                        if (postedValue.IsGuid())
                        {
                            var po = participationObjects.First(x => x.Uid == postedValue.ToGuid());

                            registration.ProcessRegistrationAssignment(
                                po,
                                SessionVariables.CurrentRegistrant,
                                submittedRecord,
                                1,
                                i,
                                participationObject.GetActivityGroupId(),
                                amountCharged: po.Cost,
                                periodId: period?.ID,
                                registeredDates: null,
                                loggedInUser: SessionVariables.CurrentUser,
                                msgs: msgs,
                                dc: dc);
                        }
                    }

                    // make sure the number of final registered programs is equal to the number of programs that is in the cart + what already exists
                    var registeredPOs = GetCurrentlyRegisteredForParticipationObjects(registration, msgs, dc);

                    if (registeredPOs.Count != participationObjects.Count)
                        msgs.Add(
                            $"Please enter all of your {UILabel.ActivityLabel().ToLower()} preferences",
                            MessageType.UserError);
                }
                else
                {
                    participationObjects.ForEach(po =>
                    {
                        registration.ProcessRegistrationAssignment(
                            po,
                            SessionVariables.CurrentRegistrant,
                            submittedRecord,
                            1,
                            participationObject.GetActivityGroupId(),
                            amountCharged: po.Cost,
                            periodId: period?.ID,
                            loggedInUser: SessionVariables.CurrentUser,
                            msgs: msgs,
                            dc: dc);
                    });
                }
            }
        }

        public static List<ParticipationObject> GetAllParticipationObjectsForCheckout(Registration registration,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            var participationObjects = new List<ParticipationObject>();

            participationObjects.AddRange(SessionVariables.Cart.GetCartItems(msgs, dc)
                .Where(x => x.ParticipationObjectTypeId == ParticipationObjectType.Activity)
                .Select(x => x.GetParticipationObject(msgs, dc))
                .Where(x => x.GetActivity().ActivityGroupId.HasValue));

            if (registration.IsNotEmpty())
                participationObjects.AddRange(GetCurrentlyRegisteredForParticipationObjects(registration, msgs, dc));

            return participationObjects.DistinctBy(x => x.ObjectID).OrderBy(x => x.Name).ToList();
        }

        private static List<ParticipationObject> GetCurrentlyRegisteredForParticipationObjects(Registration registration,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            var ignoredTypes = new List<RegistrationAssignmentType>
            {
                RegistrationAssignmentType.Declined,
                RegistrationAssignmentType.Withdrawn,
                RegistrationAssignmentType.Invited,
                RegistrationAssignmentType.NotSet,
            };

            return registration.GetRegistrantActivities(msgs, dc)
                .Where(x => x.AssignmentTypeId.NotIn(ignoredTypes))
                .Where(x => x.ParticipationObjectTypeId == ParticipationObjectType.Activity)
                .Select(x => x.GetParticipationObject(msgs, dc))
                .ToList();
        }

        public static bool IsValidLogin(AccountType accountType)
        {
            return SessionVariables.CurrentUser.IsNotEmpty() && SessionVariables.CurrentAccount.IsNotEmpty() &&
                   SessionVariables.CurrentAccount.AccountTypeId == accountType;
        }

        public static void ResetRegistrationSession()
        {
            SessionVariables.CurrentRegistrant = null;
            SessionVariables.Cart = null;
        }

        public static (ParticipationObject, RegistrationAssignmentType) GetParticipationObjectFromRegistration(
            Registration registration, SystemMessages msgs)
        {
            var registrantActivities = registration.GetRegistrantActivities(msgs);

            var registrantActivity = registrantActivities.Count == 1
                ? registrantActivities.Single()
                : registrantActivities.First(x => x.ParticipationObjectTypeId == ParticipationObjectType.ActivityGroup);

            var assignmentType = registrantActivity.AssignmentTypeId;

            return (registrantActivity.GetParticipationObject(msgs), assignmentType);
        }

        public static Dictionary<QuestionDataType, QuestionDataType> Parent1InfoToMainRegistrantMap
        {
            get
            {
                return new Dictionary<QuestionDataType, QuestionDataType>
                        {
                            {QuestionDataType.RegistrantFirstName, QuestionDataType.Parent1FirstName},
                            {QuestionDataType.RegistrantMiddleName, QuestionDataType.Parent1MiddleName},
                            {QuestionDataType.RegistrantLastName, QuestionDataType.Parent1LastName},
                            {QuestionDataType.RegistrantEmail, QuestionDataType.Parent1Email},
                            {QuestionDataType.RegistrantHomePhone, QuestionDataType.Parent1HomePhone},
                            {QuestionDataType.RegistrantMobilePhone, QuestionDataType.Parent1MobilePhone},
                            {QuestionDataType.RegistrantWorkPhone, QuestionDataType.Parent1WorkPhone},
                            {QuestionDataType.RegistrantEmployer, QuestionDataType.Parent1Employer},
                            {QuestionDataType.RegistrantParentRelationship,QuestionDataType.Parent1RelationshipToRegistrant},
                        };
            }
        }

        public static Dictionary<QuestionDataType, QuestionDataType> Parent1AddressToMainRegistrantMap
        {
            get
            {
                return new Dictionary<QuestionDataType, QuestionDataType>
                        {
                            {QuestionDataType.RegistrantAddress1, QuestionDataType.Parent1Address1},
                            {QuestionDataType.RegistrantAddress2, QuestionDataType.Parent1Address2},
                            {QuestionDataType.RegistrantCity, QuestionDataType.Parent1City},
                            {QuestionDataType.RegistrantState, QuestionDataType.Parent1State},
                            {QuestionDataType.RegistrantPostalCode, QuestionDataType.Parent1PostalCode},
                        };
            }
        }

        public static Dictionary<QuestionDataType, QuestionDataType> Parent2InfoToMainRegistrantMap
        {
            get
            {
                return new Dictionary<QuestionDataType, QuestionDataType>
                            {
                                {QuestionDataType.RegistrantFirstName, QuestionDataType.Parent2FirstName},
                                {QuestionDataType.RegistrantMiddleName, QuestionDataType.Parent2MiddleName},
                                {QuestionDataType.RegistrantLastName, QuestionDataType.Parent2LastName},
                                {QuestionDataType.RegistrantEmail, QuestionDataType.Parent2Email},
                                {QuestionDataType.RegistrantHomePhone, QuestionDataType.Parent2HomePhone},
                                {QuestionDataType.RegistrantMobilePhone, QuestionDataType.Parent2MobilePhone},
                                {QuestionDataType.RegistrantWorkPhone, QuestionDataType.Parent2WorkPhone},
                                {QuestionDataType.RegistrantEmployer, QuestionDataType.Parent2Employer},
                                {QuestionDataType.RegistrantParentRelationship,QuestionDataType.Parent2RelationshipToRegistrant},
                            };
            }
        }

        /// <summary>
        /// only should be used when mapping from the student record to parent2 
        /// otherwise you would overwrite the address info that was copied from parent 1 into the student
        /// </summary>
        public static Dictionary<QuestionDataType, QuestionDataType> Parent2AddressToMainRegistrantMap
        {
            get
            {
                return new Dictionary<QuestionDataType, QuestionDataType>
                        {
                            {QuestionDataType.RegistrantAddress1, QuestionDataType.Parent2Address1},
                            {QuestionDataType.RegistrantAddress2, QuestionDataType.Parent2Address2},
                            {QuestionDataType.RegistrantCity, QuestionDataType.Parent2City},
                            {QuestionDataType.RegistrantState, QuestionDataType.Parent2State},
                            {QuestionDataType.RegistrantPostalCode, QuestionDataType.Parent2PostalCode},
                        };
            }
        }

        public static void MapParent1AnswersToStudent(Registrant parentRegistrant, Registrant student, List<FormQuestion> allQuestions, WebUser currentUser, SystemMessages msgs, DatabaseConnection dc)
        {
            // copy parent answers to student for home address, emergency contacts
            parentRegistrant.GetAnswers(msgs, dc)
                .Where(answer =>
                    answer.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.HomeAddress ||
                    answer.FormQuestionGroupQuestionGroupDataTypeId.In(FormQuestionService
                        .GetEmergencyContactQuestionGroups())
                )
                .ToList()
            .ForEach(answer =>
            {
                student.AddOrUpdateAnswer(
                answer.Text.IfEmpty("").Trim(),
                    answer.ToFormQuestion(msgs: msgs, dc: dc),
                    currentUser,
                    msgs,
                    dc);
            });

            foreach (var key in Parent1InfoToMainRegistrantMap.Keys)
                student.MapAndAddAnswer(
                    allQuestions,
                    parentRegistrant,
                    key,
                    Parent1InfoToMainRegistrantMap[key],
                    currentUser,
                    msgs,
                    dc);

            foreach (var key in Parent1AddressToMainRegistrantMap.Keys)
                student.MapAndAddAnswer(
                    allQuestions,
                    parentRegistrant,
                    key,
                    Parent1AddressToMainRegistrantMap[key],
                    currentUser,
                    msgs,
                    dc);
        }
        
        public static void MapParent1AllAnswersToStudent(Registrant parentRegistrant, Registrant student, List<FormQuestion> allQuestions, WebUser currentUser, SystemMessages msgs, DatabaseConnection dc)
        {
            // copy parent answers to student for home address, emergency contacts
            parentRegistrant.GetAnswers(msgs, dc)
                .ToList()
                .ForEach(answer =>
                {
                    student.AddOrUpdateAnswer(
                        answer.Text.IfEmpty("").Trim(),
                        answer.ToFormQuestion(msgs: msgs, dc: dc),
                        currentUser,
                        msgs,
                        dc);
                });

            foreach (var key in Parent1InfoToMainRegistrantMap.Keys)
                student.MapAndAddAnswer(
                    allQuestions,
                    parentRegistrant,
                    key,
                    Parent1InfoToMainRegistrantMap[key],
                    currentUser,
                    msgs,
                    dc);
        }

        public static void MapParent2AnswersToStudent(Registrant parent2Registrant, Registrant student, List<FormQuestion> allQuestions, WebUser currentUser, SystemMessages msgs, DatabaseConnection dc)
        {
            foreach (var key in Parent2InfoToMainRegistrantMap.Keys)
                student.MapAndAddAnswer(
                    allQuestions,
                    parent2Registrant,
                    key,
                    Parent2InfoToMainRegistrantMap[key],
                    currentUser,
                    msgs,
                    dc);
        }
    }
}