using System.Linq;
using System.Web.Mvc;
using AfterSchool.Helpers;
using AfterSchool.Models;
using AfterSchoolLibrary.FeatureFlag;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Utilities = BasicLibrary.Utilities;

namespace AfterSchool.Controllers
{
    public class VolunteerRegistrationController : BaseController
    {
        private readonly IFeatureFlagRepository featureFlagRepository;

        public VolunteerRegistrationController(IFeatureFlagRepository featureFlagRepository)
        {
            this.featureFlagRepository = featureFlagRepository;
        }

        [HttpGet]
        public ActionResult GetStarted(string regaccount)
        {
            var msgs = new SystemMessages();

            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;

            if (currentUser.IsNotEmpty() && currentAccount.IsNotEmpty())
            {
                var registrantAccount = currentAccount;

                if (currentAccount.AccountTypeId != AccountType.Registrant)
                {
                    //check if the user has a parent account
                    registrantAccount = currentUser.GetRegistrantAccount(msgs);

                    // if no parent account, then create one real quick
                    if (registrantAccount.IsEmpty())
                        registrantAccount =
                            currentUser.AddNewRegistrantAccount(UserType.Owner, true, true, currentUser, msgs);
                }

                var masterRegistrant = SessionVariables.CurrentUser.GetMasterRegistrant(msgs);

                if (masterRegistrant.IsNotEmpty())
                    registrantAccount = masterRegistrant.GetAccount(msgs);

                if (registrantAccount.ID != currentAccount.ID)
                    LoadSessionInfo(
                        currentUser,
                        registrantAccount,
                        false,
                        msgs);

                var hasCompletedParentProfile = masterRegistrant.IsNotEmpty() && masterRegistrant.ProfileStatusId == RegistrantProfileStatus.Completed &&
                                                masterRegistrant.IsParentOrGuardian;
                if (hasCompletedParentProfile)
                    return RedirectToAction("Select");
            }

            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();

            Account registrationAccount = null;

            if (regaccount.IsGuid())
                registrationAccount = Account.LoadByUid(regaccount.ToGuid(), msgs);

            var noActivityIsSelected = participationObject.IsEmpty() || !participationObject.ObjectID.HasValue;
            if (noActivityIsSelected)
            {
                if (SessionVariables.RegistrationStartPageAccount.IsNotEmpty() || registrationAccount.IsNotEmpty())
                    return Redirect(SessionVariables.RegistrationStartPageAccount?.HomePageUrl ?? registrationAccount?.HomePageUrl);

                return HttpNotFound();
            }

            var questions = RegistrationCommon.GetAllDefaultPersonalInformationQuestions(msgs);

            var getStartedAssembler = new GetStartedAssembler(featureFlagRepository);
            var result = getStartedAssembler.AssembleGetStartedResult(msgs, participationObject, registrationAccount,
                SessionVariables.CurrentUser, questions, false, Request.Params);
            ViewBag.AccountProfile = participationObject.OwnerAccount.GetAccountProfile(msgs);
            return View(result);
        }

        [HttpPost]
        public ActionResult GetStarted(string regaccount, string password1, string password2, string agree, string textnotifications)
        {
            var msgs = new SystemMessages();
            var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
            var currentUser = SessionVariables.CurrentUser;

            var allQuestionsForQuestionGroup = RegistrationCommon.GetAllDefaultPersonalInformationQuestions(msgs);
            var getStartedAssembler = new GetStartedAssembler(featureFlagRepository);
            var qvms = getStartedAssembler.FilterAndTransformQuestionsForParentRegistration(currentUser, allQuestionsForQuestionGroup, Request.Params);

            if (!agree.IsBoolean())
                msgs.Add("You must agree to the terms of service", MessageType.UserError);

            qvms.ForEach(qvm =>
            {
                if (qvm.Question.IsRequired && qvm.Value.IsEmpty())
                    msgs.Add($"{qvm.Question.Text} is required", MessageType.UserError);

                switch (qvm.Question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantMobilePhone:
                        if (qvm.Value.IsNotEmpty() && !qvm.Value.Trim().IsRegExMatch(Utilities.PhoneRegEx))
                            msgs.Add($"{qvm.Question.Text} must be a valid phone number", MessageType.UserError);
                        break;
                }
            });

            if (currentUser.IsEmpty())
            {
                if (password1.IsEmpty())
                    msgs.Add("Password 1 is required", MessageType.UserError);
                if (password1.Length < 8)
                    msgs.Add("Password needs to have at least 8 characters", MessageType.UserError);
                if (password2.IsEmpty())
                    msgs.Add("Password 2 is required", MessageType.UserError);
                if (password1 != password2)
                    msgs.Add("Passwords must match", MessageType.UserError);
                if (AfterSchoolLibrary.Helpers.Utilities.IsProduction)
                    msgs.AddRange(RecaptchaHelper.Validate(Request["g-recaptcha-response"]));
            }

            if (msgs.HasNoErrors())
            {
                var firstName = qvms.First(qvm =>
                    qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName);
                var lastName = qvms.First(qvm =>
                    qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName);
                var email = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail);
                var phone = qvms.First(qvm =>
                    qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone);

                WebUser user = null;
                Account account = null;
                Registrant registrant = null;

                Database.ExecuteTransaction
                (
                    (amsgs, adc) =>
                    {
                        user = RegistrationCommon.CreateOrUpdateUser(
                            currentUser,
                            firstName.Value.Trim(),
                            lastName.Value.Trim(),
                            email.Value.Trim(),
                            phone.Value.Trim(),
                            password1,
                            textnotifications.IfBlank("0").ToBoolean(),
                            amsgs,
                            adc);


                        if (amsgs.HasNoErrors() && user.IsNotEmpty())
                        {
                            account = RegistrationCommon.CreateOrUpdateAccount(user, amsgs, adc);

                            // LOG: Someone Created Account AccountName
                            SessionVariables.WebSession.LogActivity(
                                account,
                                currentUser,
                                WebSessionActionType.Created,
                                ObjectType.Account,
                                objectId: account.ID.Value,
                                objectText: account.Name
                            );

                            // LOG: Someone Created User UserName
                            SessionVariables.WebSession.LogActivity(
                                account,
                                currentUser,
                                WebSessionActionType.SignedUp,
                                ObjectType.WebUser,
                                objectId: user.ID.Value,
                                objectText: user.FullName
                            );
                        }

                        if (amsgs.HasNoErrors())
                            registrant = RegistrationCommon.CreateOrUpdateRegistrant(account, user, amsgs, adc);

                        if (amsgs.HasNoErrors())
                            RegistrationCommon.AddOrUpdateStandardAnswers(qvms, registrant, user, amsgs, adc);
                    },
                    smsgs =>
                    {
                        msgs.AddRange(smsgs);
                        LoadSessionInfo(user, account);
                    },
                    failureMessages => msgs.AddRange(failureMessages),
                    exceptionMessages => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(exceptionMessages)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("AddHomeAddress");

            var registrationAccount = regaccount.IsGuid() ? Account.LoadByUid(regaccount.ToGuid(), msgs) : null;
            var questions = RegistrationCommon.GetAllDefaultPersonalInformationQuestions(msgs);
            var result = getStartedAssembler.AssembleGetStartedResult(msgs, participationObject, registrationAccount,
                SessionVariables.CurrentUser, questions, false, Request.Params);
            ViewBag.AccountProfile = participationObject.OwnerAccount.GetAccountProfile(msgs);
            return View(result);
        }

        [HttpGet]
        public ActionResult Select()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AddHomeAddress()
        {
            return View("Select");
        }
    }
}