using System;
using System.Collections.Generic;
using System.Linq;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

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
            var administratorDefaultForm = LoadAdministratorDefaultForm(msgs);
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

        private static Form LoadAdministratorDefaultForm(SystemMessages msgs)
        {
            var administratorAccount = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs).FirstOrDefault();
            return administratorAccount?.DefaultFormId != null
                ? Form.Load(administratorAccount.DefaultFormId.Value, msgs)
                : null;
        }

        public static WebUser CreateOrUpdateUser(WebUser currentUser, string firstName, string lastName, string email,
            string phone, string password, bool allowTextMessages, SystemMessages msgs, DatabaseConnection dc = null)
        {
            WebUser user = null;

            if (currentUser.IsEmpty())
            {
                user = WebUserService.CreateNew(
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
            else
            {
                user = SessionVariables.CurrentUser;
                user.Phone = phone.Trim();
                user.AllowTextMessages = allowTextMessages;
                user.LastModifiedBy = currentUser.ID;
                user.LastModifiedDate = DateTime.UtcNow;
                msgs.AddRange(user.Save(dc));
            }

            return user;
        }

        public static Account CreateOrUpdateAccount(WebUser user, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var account = user.GetRegistrantAccount(msgs, dc);

            // create account
            if (account.IsEmpty())
                account = user.AddNewRegistrantAccount(
                    UserType.Owner,
                    true,
                    true,
                    user,
                    msgs,
                    dc);

            return account;
        }

        public static Registrant CreateOrUpdateRegistrant(Account account, WebUser user, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            var registrant = account.GetFirstParentOrGuardianRegistrant(msgs, dc);

            if (registrant.IsEmpty())
                // create parent registrant record
                registrant = account.CreateRegistrant(
                    user,
                    user.AsList(),
                    RegistrantType.Individual,
                    true,
                    true,
                    false,
                    null,
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
    }
}