using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;

namespace AfterSchool.Controllers
{
    public class AccountController : BaseController
    {

        #region GetStartedParent

        private List<FormQuestion> GetStartedParentViewBag(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var personalContactGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = personalContactGroup.GetAllQuestions(msgs);

            questions = questions.Where(question =>
                    question.QuestionDataTypeId != QuestionDataType.RegistrantOrganization &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantGrade &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantGender &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantDateOfBirth &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantEthnicity &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantTShirtSize
                )
                .ToList();

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantEmail:
                    case QuestionDataType.RegistrantMobilePhone:
                        question.IsRequired = true;
                        break;
                }
            });

            ViewBag.Cart = SessionVariables.Cart;

            return questions;
        }

        [HttpGet]
        public ActionResult GetStarted(string regaccount)
        {
            if (SessionVariables.CurrentUser.IsNotEmpty() && SessionVariables.CurrentAccount.AccountTypeId == AccountType.Registrant)
                return RedirectToAction("select");

            var msgs = new SystemMessages();
            var questions = GetStartedParentViewBag(msgs);

            ViewBag.Messages = msgs;

            if (regaccount.IsGuid())
                ViewBag.RegistrationAccount = Account.LoadByUid(regaccount.ToGuid(), msgs);

            return View(questions.Select(question => new FormQuestionViewModel(question)).ToList());
        }

        [HttpPost]
        public ActionResult GetStarted(string regaccount, string password1, string password2, string agree, string textnotifications)
        {
            var msgs = new SystemMessages();

            var questions = GetStartedParentViewBag(msgs);

            questions.ForEach(question =>
            {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantMobilePhone:
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantWorkPhone:
                        if (answer.IsNotEmpty() && !answer.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{question.Text} must be a valid phone number", MessageType.UserError);
                        break;

                }
            });

            if (password1.IsEmpty())
                msgs.Add("Password 1 is required", MessageType.UserError);
            if (password1.Length < 8)
                msgs.Add("Password needs to have at least 8 characters", MessageType.UserError);
            if (password2.IsEmpty())
                msgs.Add("Password 2 is required", MessageType.UserError);
            if (password1 != password2)
                msgs.Add("Passwords must match", MessageType.UserError);
            if (!agree.IsBoolean())
                msgs.Add("You must agree to the terms of service", MessageType.UserError);

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
            {
                var firstName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName);
                var lastName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName);
                var email = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail);
                var phone = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone);

                WebUser user = null;
                Account account = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // create user
                        user = WebUserService.CreateNew(
                            firstName: firstName.Value,
                            lastName: lastName.Value,
                            email: email.Value.Trim(),
                            phone: phone.Value.Trim(),
                            password: password1,
                            isActive: true,
                            isRegistered: true,
                            isVerified: false,
                            allowTextMessages: textnotifications.IfBlank("0").ToBoolean(),
                            loggedInUser: null,
                            msgs: amsgs,
                            dc: adc);

                        // create account
                        account = user.AddNewRegistrantAccount(
                            userType: UserType.Owner,
                            isActive: true,
                            isAfterSchool: SessionVariables.IsAfterSchool.Value,
                            loggedInUser: user,
                            msgs: amsgs,
                            dc: adc);

                        // create parent registrant record
                        var registrant = account.CreateRegistrant(
                            registrantOwner: user,
                            canBeAccessedBy: user.AsList(),
                            registrantType: RegistrantType.Individual,
                            isMaster: true,
                            isParentOrGuardian: true,
                            allowTextMessages: textnotifications.IfBlank("0").ToBoolean(),
                            schoolLocationId: null,
                            profileStatus: RegistrantProfileStatus.Started,
                            loggedInUser: user,
                            msgs: amsgs,
                            dc: adc);

                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            registrant.AddAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(),
                                question: qvm.Question,
                                loggedInUserId: user.ID.Value,
                                msgs: amsgs,
                                dc: adc);
                        });
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        SessionVariables.CurrentUser = user;
                        SessionVariables.CurrentAccount = account;
                        SessionVariables.AccountUserType = user.GetUserType(account);
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("addhomeaddress");

            return View(qvms);
        }

        #endregion

        #region AddHomeAddress

        private List<FormQuestion> AddHomeAddressViewBag(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var homeAddressQuestionGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.HomeAddress);
            var questions = homeAddressQuestionGroup.GetAllQuestions(msgs);

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantAddress2:
                        question.IsRequired = false;
                        break;
                    default:
                        question.IsRequired = true;
                        break;
                }
            });

            ViewBag.Cart = SessionVariables.Cart;

            return questions;
        }

        [HttpGet]
        public ActionResult AddHomeAddress()
        {
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var questions = AddHomeAddressViewBag(msgs);

            ViewBag.Messages = msgs;

            return View(questions.Select(question => question.ToFormQuestionViewModel()).ToList());
        }

        [HttpPost]
        public ActionResult AddHomeAddress(string id)
        {
            var msgs = new SystemMessages();
            var questions = AddHomeAddressViewBag(msgs);
            var parentRegistrant = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant();

            questions.ForEach(question =>
            {
                if (question.IsRequired && Request[question.ToFormQuestionViewModel().Name].IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);
            });

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            parentRegistrant.AddAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(),
                                question: qvm.Question,
                                loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                                msgs: amsgs,
                                dc: adc);
                        });
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("addparent");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region AddParent

        private List<FormQuestion> AddParentViewBag(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var personalContactGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = personalContactGroup.GetAllQuestions(msgs);

            questions = questions.Where(question =>
                    question.QuestionDataTypeId != QuestionDataType.RegistrantOrganization &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantGrade &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantGender &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantDateOfBirth &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantEthnicity &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantTShirtSize
                )
                .ToList();

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    // case QuestionDataType.RegistrantEmail:
                    case QuestionDataType.RegistrantMobilePhone:
                        question.IsRequired = true;
                        break;
                }
            });

            ViewBag.Cart = SessionVariables.Cart;

            return questions;
        }

        [HttpGet]
        public ActionResult AddParent()
        {
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var parentOrGuardian = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant();
            var questions = AddParentViewBag(msgs);
            var values = new NameValueCollection();

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantHomePhone:
                        values.Add(question.ToFormQuestionViewModel().Id, parentOrGuardian.GetAnswer(question.QuestionDataTypeId)?.Text);
                        break;
                }
            });

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(values)).ToList();

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        [HttpPost]
        public ActionResult AddParent(string id, string createuseraccount)
        {
            var msgs = new SystemMessages();
            var questions = AddParentViewBag(msgs);

            questions.ForEach(question =>
            {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantWorkPhone:
                    case QuestionDataType.RegistrantMobilePhone:
                        if (answer.IsNotEmpty() && !answer.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{question.Text} must be a valid phone number", MessageType.UserError);
                        break;

                }
            });

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
            {
                Registrant otherParent = null;
                WebUser otherParentUser = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        var canBeAccessedBy = SessionVariables.CurrentUser.AsList();

                        // create student account if requestioned by parent
                        if (createuseraccount.IsNumber())
                        {
                            var firstName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName).Value;
                            var lastName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName).Value;
                            var email = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail).Value;
                            var phone = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone).Value;

                            otherParentUser = SessionVariables.CurrentAccount.InviteUser(
                                 firstName: firstName,
                                 lastName: lastName,
                                 email: email,
                                 phone: phone,
                                 message: null,
                                 userType: UserType.Student,
                                 loggedInUser: SessionVariables.CurrentUser,
                                 msgs: amsgs,
                                 dc: adc);

                            canBeAccessedBy.Add(otherParentUser);
                        }

                        // create parent registrant record
                        otherParent = SessionVariables.CurrentAccount.CreateRegistrant(
                            registrantOwner: otherParentUser,
                            canBeAccessedBy: canBeAccessedBy,
                            registrantType: RegistrantType.Individual,
                            isMaster: true,
                            isParentOrGuardian: true,
                            allowTextMessages: false,
                            schoolLocationId: null,
                            profileStatus: RegistrantProfileStatus.Started,
                            loggedInUser: SessionVariables.CurrentUser,
                            msgs: amsgs,
                            dc: adc);

                        qvms.ForEach(qvm =>
                        {
                            otherParent.AddAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(),
                                question: qvm.Question,
                                loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                                msgs: amsgs,
                                dc: adc);
                        });
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("addemergencycontacts");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region AddEmergencyContacts

        private List<FormQuestion> AddEmergencyContactsViewBag(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var emergencyContactsQuestionGroups = form.GetQuestionGroups(msgs)
                .Where(group =>
                    group.QuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact1 ||
                    group.QuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact2)
                .ToList();
            var questions = new List<FormQuestion>();

            emergencyContactsQuestionGroups.ForEach(group => questions.AddRange(group.GetAllQuestions(msgs)));
            questions.Where(question => question.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact1)
                .ToList()
                .ForEach(question => { question.IsRequired = true; });

            ViewBag.Cart = SessionVariables.Cart;

            return questions;
        }

        [HttpGet]
        public ActionResult AddEmergencyContacts()
        {
            var results = CheckRedirect();
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var questions = AddEmergencyContactsViewBag(msgs);

            ViewBag.Messages = msgs;

            return View(questions.Select(question => question.ToFormQuestionViewModel()).ToList());
        }

        [HttpPost]
        public ActionResult AddEmergencyContacts(string id)
        {
            var msgs = new SystemMessages();
            var questions = AddEmergencyContactsViewBag(msgs);
            var parentRegistrant = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant();
            var phones = new List<string>();

            questions.ForEach(question =>
            {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.EmergencyContact1Phone:
                    case QuestionDataType.EmergencyContact2Phone:
                        if (answer.IsNotEmpty() && !answer.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{question.FormQuestionGroupQuestionGroupDataTypeId.ToString().SplitByUpperCase()} {question.Text} must be a valid phone number", MessageType.UserError);

                        if (answer.IsNotEmpty())
                            phones.Add(answer);

                        break;

                }
            });

            if (phones.Count == 2 && phones[0] == phones[1])
                msgs.Add("Emergency phone numbers should not be the same", MessageType.UserError);

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // mark parent profile as completed since they've went through all the steps
                        parentRegistrant.SetProfileAsCompleted(SessionVariables.CurrentUser, amsgs, adc);

                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            parentRegistrant.AddAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(),
                                question: qvm.Question,
                                loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                                msgs: amsgs,
                                dc: adc);
                        });
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("addstudent");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region AddStudent

        private List<FormQuestion> AddStudentViewBag(Form administratorAccountForm, ParticipationObject participationObject, SystemMessages msgs)
        {
            var studentQuestionGroup = administratorAccountForm.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = studentQuestionGroup.GetAllQuestions(msgs);

            questions = questions.Where(question =>
                    question.QuestionDataTypeId != QuestionDataType.RegistrantOrganization &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantWorkPhone
                )
                .ToList();

            questions.ForEach(question =>
                {
                    switch (question.QuestionDataTypeId)
                    {
                        case QuestionDataType.RegistrantFirstName:
                        case QuestionDataType.RegistrantLastName:
                        case QuestionDataType.RegistrantGrade:
                        case QuestionDataType.RegistrantDateOfBirth:
                        case QuestionDataType.RegistrantGender:
                        case QuestionDataType.RegistrantTShirtSize:
                            question.IsRequired = true;
                            break;
                        case QuestionDataType.RegistrantHomePhone:
                        case QuestionDataType.RegistrantEmail:
                            question.IsRequired = false;
                            break;
                    }
                });

            var schoolLocation = new Location();

            if (SessionVariables.RegistrationStartPageAccount.AccountTypeId == AccountType.School)
                schoolLocation = SessionVariables.RegistrationStartPageAccount.GetDefaultLocation(msgs);

            ViewBag.School = schoolLocation;
            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.ShowStepNumber = SessionVariables.CurrentAccount.GetChildrenRegistrants(msgs).Count == 0;

            return questions;
        }

        [HttpGet]
        public ActionResult AddStudent()
        {
            var results = CheckRedirect();
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var adminAccountForm = GetAdministratorAccountForm(msgs);
            var parentOrGuardian = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant();
            var participationObject = SessionVariables.Cart.GetCartItems(msgs).First().GetParticipationObject(msgs);
            var questions = AddStudentViewBag(adminAccountForm, participationObject, msgs);
            var values = new NameValueCollection();

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantEmail:
                    case QuestionDataType.RegistrantMobilePhone:
                    case QuestionDataType.RegistrantHomePhone:
                        values.Add(question.ToFormQuestionViewModel().Id, parentOrGuardian.GetAnswer(question.QuestionDataTypeId)?.Text);
                        break;
                }
            });

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(values)).ToList();

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        [HttpPost]
        public ActionResult AddStudent(string id, string samehomeaddress, string location, string createuseraccount)
        {
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var adminAccountForm = GetAdministratorAccountForm(msgs);
            var parentRegistrant = SessionVariables.CurrentAccount.GetFirstParentOrGuardianRegistrant(msgs);
            var parentRegistrant2 = SessionVariables.CurrentAccount.GetSecondParentOrGuardianRegistrant(msgs);
            var participationObject = SessionVariables.Cart.GetCartItems(msgs).First().GetParticipationObject(msgs);
            var questions = AddStudentViewBag(adminAccountForm, participationObject, msgs);

            questions.ForEach(question =>
            {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantWorkPhone:
                    case QuestionDataType.RegistrantMobilePhone:
                        if (answer.IsNotEmpty() && !answer.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{question.Text} must be a valid phone number", MessageType.UserError);
                        break;

                }
            });

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
            {
                Registrant student = null;
                WebUser studentUser = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        var canBeAccessedBy = SessionVariables.CurrentUser.AsList();

                        // create student account if requestioned by parent
                        if (createuseraccount.IsNumber())
                        {
                            var firstName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName).Value;
                            var lastName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName).Value;
                            var email = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail).Value;
                            var phone = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone).Value;

                            studentUser = SessionVariables.CurrentAccount.InviteUser(
                                 firstName: firstName,
                                 lastName: lastName,
                                 email: email,
                                 phone: phone,
                                 message: null,
                                 userType: UserType.Student,
                                 loggedInUser: SessionVariables.CurrentUser,
                                 msgs: amsgs,
                                 dc: adc);

                            canBeAccessedBy.Add(studentUser);
                        }

                        // create student registrant record
                        student = SessionVariables.CurrentAccount.CreateRegistrant(
                            registrantOwner: studentUser,
                            canBeAccessedBy: canBeAccessedBy,
                            registrantType: RegistrantType.Individual,
                            isMaster: true,
                            isParentOrGuardian: false,
                            allowTextMessages: false,
                            schoolLocationId: location.IsNumber() ? location.ToInteger() : new int?(),
                            profileStatus: RegistrantProfileStatus.Started,
                            loggedInUser: SessionVariables.CurrentUser,
                            msgs: amsgs,
                            dc: adc);

                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            student.AddAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(),
                                question: qvm.Question,
                                loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                                msgs: amsgs,
                                dc: adc);
                        });

                        // copy parent answers to student for home address, emergency contacts
                        parentRegistrant.GetAnswers(amsgs, adc)
                            .Where(answer =>
                                answer.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.HomeAddress ||
                                answer.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact1 ||
                                answer.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact2
                            )
                            .ToList()
                            .ForEach(answer =>
                            {
                                student.AddAnswer(
                                    answerText: answer.Text.IfEmpty("").Trim(),
                                    question: answer.ToFormQuestion(msgs: amsgs, dc: adc),
                                    loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                                    msgs: amsgs,
                                    dc: adc);
                            });

                        // copy parent info over to student parent info
                        var allquestions = adminAccountForm.GetAllQuestions(amsgs, adc);

                        var parent1Map = new Dictionary<QuestionDataType, QuestionDataType>()
                        {
                            { QuestionDataType.RegistrantFirstName,  QuestionDataType.Parent1FirstName },
                            { QuestionDataType.RegistrantLastName,  QuestionDataType.Parent1LastName },
                            { QuestionDataType.RegistrantEmail,  QuestionDataType.Parent1Email },
                            { QuestionDataType.RegistrantHomePhone,  QuestionDataType.Parent1HomePhone },
                            { QuestionDataType.RegistrantMobilePhone,  QuestionDataType.Parent1MobilePhone },
                            { QuestionDataType.RegistrantWorkPhone,  QuestionDataType.Parent1WorkPhone },
                        };

                        foreach (var key in parent1Map.Keys)
                        {
                            student.MapAndAddAnswer(
                               allquestions,
                               parentRegistrant,
                               key,
                               parent1Map[key],
                               SessionVariables.CurrentUser,
                               amsgs,
                               adc);
                        }

                        var parent1AddressMap = new Dictionary<QuestionDataType, QuestionDataType>()
                        {
                            { QuestionDataType.RegistrantAddress1,  QuestionDataType.Parent1Address1 },
                            { QuestionDataType.RegistrantAddress2,  QuestionDataType.Parent1Address2 },
                            { QuestionDataType.RegistrantCity,  QuestionDataType.Parent1City },
                            { QuestionDataType.RegistrantState,  QuestionDataType.Parent1State },
                            { QuestionDataType.RegistrantPostalCode,  QuestionDataType.Parent1PostalCode },
                        };

                        foreach (var key in parent1AddressMap.Keys)
                        {
                            student.MapAndAddAnswer(
                               allquestions,
                               parentRegistrant,
                               key,
                               parent1AddressMap[key],
                               SessionVariables.CurrentUser,
                               amsgs,
                               adc);
                        }

                        if (parentRegistrant2.IsNotEmpty())
                        {
                            // get parent 2 info
                            var parent2Map = new Dictionary<QuestionDataType, QuestionDataType>()
                            {
                                { QuestionDataType.RegistrantFirstName,  QuestionDataType.Parent2FirstName },
                                { QuestionDataType.RegistrantLastName,  QuestionDataType.Parent2LastName },
                                { QuestionDataType.RegistrantEmail,  QuestionDataType.Parent2Email },
                                { QuestionDataType.RegistrantHomePhone,  QuestionDataType.Parent2HomePhone },
                                { QuestionDataType.RegistrantMobilePhone,  QuestionDataType.Parent2MobilePhone },
                                { QuestionDataType.RegistrantWorkPhone,  QuestionDataType.Parent2WorkPhone },
                            };

                            foreach (var key in parent2Map.Keys)
                            {
                                student.MapAndAddAnswer(
                                   allquestions,
                                   parentRegistrant2,
                                   key,
                                   parent2Map[key],
                                   SessionVariables.CurrentUser,
                                   amsgs,
                                   adc);
                            }
                        }

                        // TODO: maybe should get sibling and copy health info
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("addhealth");

            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region AddHealth

        private List<FormQuestion> AddHealthViewBag(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var healthQuestionGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.HealthInformation);
            var questions = healthQuestionGroup.GetAllQuestions(msgs);

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    default:
                        question.IsRequired = false;
                        break;
                }
            });

            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.ShowStepNumber = SessionVariables.CurrentAccount.GetChildrenRegistrants(msgs).Count < 2;

            return questions;
        }

        [HttpGet]
        public ActionResult AddHealth()
        {
            if (SessionVariables.CurrentUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return RedirectToAction("");

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return RedirectToAction("");

            var msgs = new SystemMessages();
            var questions = AddHealthViewBag(msgs);

            ViewBag.Student = SessionVariables.CurrentRegistrant;
            ViewBag.Messages = msgs;

            return View(questions.Select(question => new FormQuestionViewModel(question)).ToList());
        }

        [HttpPost]
        public ActionResult AddHealth(string id)
        {
            if (SessionVariables.CurrentUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return RedirectToAction("");

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return RedirectToAction("");

            var msgs = new SystemMessages();
            var questions = AddHealthViewBag(msgs);

            questions.ForEach(question =>
            {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantDoctorPhone:
                        if (answer.IsNotEmpty() && !answer.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{question.Text} must be a valid phone number", MessageType.UserError);
                        break;

                }
            });

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // mark student profile as completed since they've went through all the steps
                        SessionVariables.CurrentRegistrant.SetProfileAsCompleted(SessionVariables.CurrentUser, amsgs, adc);

                        // add answers to parent registrant record
                        // will be used later to copy information over to student records
                        qvms.ForEach(qvm =>
                        {
                            SessionVariables.CurrentRegistrant.AddAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(),
                                question: qvm.Question,
                                loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                                msgs: amsgs,
                                dc: adc);
                        });
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("register");

            ViewBag.Student = SessionVariables.CurrentRegistrant;
            ViewBag.Messages = msgs;

            return View(qvms);
        }

        #endregion

        #region SelectStudent

        [HttpGet]
        public ActionResult Select()
        {
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();

            var masterRegistrant = SessionVariables.CurrentUser.GetMasterRegistrant();
            if (!masterRegistrant.IsParentOrGuardian)
            {
                SessionVariables.CurrentRegistrant = masterRegistrant;
                return RedirectToAction("registerstudent");
            }

            var students = SessionVariables.CurrentAccount.GetChildrenRegistrants(msgs);

            ViewBag.Messages = msgs;
            ViewBag.ParticipationObject = SessionVariables.Cart.GetCartItems().FirstOrDefault()?.GetParticipationObject();

            return View(students);
        }

        private ActionResult SetStudentAction(string id)
        {
            if (id.IsGuid())
            {
                SessionVariables.CurrentRegistrant = Registrant.LoadByUid(id.ToGuid());
                return RedirectToAction("register", "account");
            }
            return RedirectToAction("select", "account");
        }

        [HttpGet]
        public ActionResult SetStudent(string id)
        {
            return HandleIsLoggedIn() ?? SetStudentAction(id);
        }

        #endregion

        #region Register

        private List<FormQuestion> RegisterViewBag(ParticipationObject participationObject, SystemMessages msgs)
        {
            var allQuestions = participationObject.Form
                .GetActiveQuestions(msgs)
                .ToList();
            var requiredQuestions = allQuestions
                .Where(question => question.IsRequired &&
                    question.FormQuestionGroupRegistrantTypeId == RegistrantType.Individual &&
                    question.QuestionDataTypeId != QuestionDataType.NotSet)
                .ToList();

            var answers = SessionVariables.CurrentRegistrant.GetAnswers(msgs);

            var questions = requiredQuestions.Where(question =>
                    answers.Find(answer => answer.QuestionDataTypeId == question.QuestionDataTypeId).IsEmpty()
                )
                .Distinct()
                .ToList();

            questions.AddRange(
                    allQuestions
                        .Where(question => question.QuestionDataTypeId == QuestionDataType.NotSet)
                        .Where(question => answers.Find(answer => answer.FormQuestionId == question.ID).IsEmpty())
                );

            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.Registrant = SessionVariables.CurrentRegistrant;

            return questions;
        }

        private ActionResult RegisterActionGet()
        {
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var participationObject = SessionVariables.Cart.GetCartItems().FirstOrDefault()?.GetParticipationObject();

            // current registrant is already registered
            if (SessionVariables.CurrentRegistrant.IsRegisteredForAndHasConsent(participationObject, msgs))
                return RedirectToAction("select");

            // need to see if there are any questions required by the form on the activity/activity group that haven't been collected on the main form

            var questions = RegisterViewBag(participationObject, msgs);

            ViewBag.QuestionViewModels = questions.Select(question => question.ToFormQuestionViewModel()).ToList();
            ViewBag.Messages = msgs;

            return View(participationObject);
        }

        [HttpGet]
        public ActionResult Register()
        {
            return HandleIsLoggedIn() ?? RegisterActionGet();
        }

        private ActionResult RegisterActionPost(string signature)
        {
            var msgs = new SystemMessages();
            var participationObject = SessionVariables.Cart.GetCartItems().FirstOrDefault()?.GetParticipationObject();

            // need to see if there are any questions required by the form on the activity/activity group that haven't been collected on the main form
            var questions = RegisterViewBag(participationObject, msgs);
            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            questions.ForEach(question =>
            {
                if (question.IsRequired && Request[question.ToFormQuestionViewModel().Name].IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);
            });

            if (signature.IsEmpty())
                msgs.Add("E-signature is required", MessageType.UserError);

            Registration registration = null;
            RegistrationPeriod period = null;
            Registrant accountRegistrantCopy = null;

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        period = participationObject.GetCurrentRegistrationPeriod(amsgs, adc);

                        qvms.Where(qvm => qvm.Question.QuestionDataTypeId != QuestionDataType.NotSet)
                        .ToList()
                        .ForEach(qvm =>
                        {
                            SessionVariables.CurrentRegistrant.AddAnswer(
                                answerText: qvm.Value,
                                question: qvm.Question,
                                loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                                msgs: amsgs,
                                dc: adc);
                        });

                        // account gets copy / snapshot of master registrant record
                        // required in order to make sure registration data doesn't change as registrant changes information in master record.
                        (accountRegistrantCopy, registration) = SessionVariables.CurrentRegistrant.GetSubmittedRecord(participationObject, amsgs, adc);

                        if (accountRegistrantCopy.IsEmpty())
                        {
                            accountRegistrantCopy = SessionVariables.CurrentRegistrant.SubmitRecordToAccount(
                                ownerAccount: participationObject.OwnerAccount,
                                loggedInUser: SessionVariables.CurrentUser,
                                msgs: amsgs,
                                dc: adc
                                );

                            // create or update registration
                            registration = accountRegistrantCopy.AddRegistrationWithESignature(
                                ownerAccountId: participationObject.OwnerAccount.ID.Value,
                                activityGroupId: participationObject.GetAnyRelatedActivityGroupId(),
                                accountActivityGroupId: participationObject.GetAccountActivityGroupId(),
                                periodId: period?.ID,
                                esignature: signature,
                                loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                                msgs: amsgs,
                                dc: adc);
                        }
                        else
                            registration.AddESignature(
                                esignature: signature,
                                loggedInUser: SessionVariables.CurrentUser,
                                msgs: amsgs,
                                dc: adc);

                        //copy all answers from the main account to the registrant & registration :)
                        SessionVariables.CurrentRegistrant.CopyAnswers(
                            copyingRegistrant: accountRegistrantCopy,
                            registration: registration,
                            loggedInUser: SessionVariables.CurrentUser,
                            msgs: amsgs,
                            dc: adc);

                        // add custom answers to questions directly to this registration
                        qvms.Where(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.NotSet)
                        .ToList()
                        .ForEach(qvm =>
                        {
                            accountRegistrantCopy.AddAnswer(
                                answerText: qvm.Value,
                                question: qvm.Question,
                                registrationId: registration.ID,
                                loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                                msgs: amsgs,
                                dc: adc);
                        });

                        // if there is a Session, then a person may set their preferences for activities in that session, 
                        // also only if the session is set to Apply
                        if (participationObject.IsActivityGroup() && participationObject.GetActivityGroup().RegistrationModeId == RegistrationMode.Apply)
                        {
                            var activities = participationObject.GetActivityGroup()
                                .GetActivities(null, false, false, null, null, false, amsgs, adc);

                            for (int i = 1; i <= activities.Count; i++)
                            {
                                var postedValue = Request[$"activity-preference-{i}"];
                                if (postedValue.IsGuid())
                                {
                                    var activity = Activity.LoadByUid(postedValue.ToGuid(), msgs);

                                    registration.ProcessRegistrationAssignment(
                                        participationObject: activity.GetParticipationObject(amsgs, adc),
                                        masterRegistrant: SessionVariables.CurrentRegistrant,
                                        registrant: accountRegistrantCopy,
                                        seatsRequested: 1,
                                        preferenceNumber: i,
                                        activityGroupId: participationObject.GetActivityGroupId(),
                                        amountCharged: participationObject.Cost,
                                        periodId: period?.ID,
                                        loggedInUser: SessionVariables.CurrentUser,
                                        msgs: amsgs,
                                        dc: adc);
                                }
                            }
                        }

                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
            {
                if (participationObject.IsFree || participationObject.RegistrationMode == RegistrationMode.Apply)
                    return RedirectToAction("finalizeregistration", new { id = registration.Uid });

                return RedirectToAction("payment", new { id = registration.Uid });
            }

            ViewBag.QuestionViewModels = qvms;
            ViewBag.Messages = msgs;

            return View(participationObject);
        }

        [HttpPost]
        public ActionResult Register(string signature)
        {
            return HandleIsLoggedIn() ?? RegisterActionPost(signature);
        }

        private ActionResult FinalizeRegistrationGet(string registrationUid)
        {
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();
            var participationObject = SessionVariables.Cart.GetCartItems(msgs).FirstOrDefault()?.GetParticipationObject(msgs);
            var canAcceptPayments = participationObject.OwnerAccount.CanProcessPayments(msgs);

            // if you can't pay later
            if (!participationObject.AllowToPayLater && canAcceptPayments)
            {
                // if it's not free and registration mode is not set to apply mode
                // if it costs something and you're forced to register then you need to pay
                if (!participationObject.IsFree && participationObject.RegistrationMode == RegistrationMode.Register)
                    return RedirectToAction("payment", new { id = registrationUid });
            }

            // need to actually register first :)
            if (!registrationUid.IsGuid())
                return RedirectToAction("register");

            // current registrant is already registered, then select a different student
            if (SessionVariables.CurrentRegistrant.IsRegisteredFor(participationObject, msgs))
                return RedirectToAction("select");

            var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);

            if (msgs.HasNoErrors())
                AccountService.ProcessRegistrationWithoutPayment(
                    checkoutObject: new CheckoutObject()
                    {
                        BillingAccount = participationObject.OwnerAccount,
                        PayingAccount = SessionVariables.CurrentAccount,
                        NumberOfSeats = 1,
                        ParticipationObject = participationObject,
                        Registration = registration,
                        MasterRegistrant = SessionVariables.CurrentRegistrant
                    },
                    sendConfirmationEmail: true,
                    loggedInUser: SessionVariables.CurrentUser,
                    msgs: msgs);

            if (msgs.HasNoErrors())
                return RedirectToAction("thankyou", new { id = registration.Uid });

            return View();
        }

        [HttpGet]
        public ActionResult FinalizeRegistration(string id)
        {
            return HandleIsLoggedIn() ?? FinalizeRegistrationGet(id);
        }

        [HttpGet]
        public ActionResult ParentalConsent(string id)
        {
            var msgs = new SystemMessages();
            var registration = Registration.LoadByUid(id.ToGuid(), msgs);

            SessionVariables.CurrentRegistrant = registration.GetRegistrant(msgs).GetMasterRegistrant(msgs);
            SessionVariables.Cart = Cart.GetNewCart(loggedInUser: SessionVariables.CurrentUser, msgs: msgs);
            SessionVariables.Cart.AddCartItem(registration.GetParticipationObject(msgs), msgs);

            return RedirectToAction("Register");
        }

        #endregion

        #region Payment

        private Registration PaymentViewBag(string id, ParticipationObject participationObject, SystemMessages msgs)
        {
            var registration = Registration.LoadByUid(id.ToGuid(), msgs);

            ViewBag.Registration = registration;
            ViewBag.Sources = SessionVariables.CurrentAccount.GetStripeSources(msgs);
            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.ParticipationObject = participationObject;
            ViewBag.Messages = msgs;
            ViewBag.AccountCanAcceptPayments = participationObject.PaymentAccount.CanProcessPayments(msgs);

            return registration;
        }

        private ActionResult PaymentActionGet(string id)
        {
            var results = CheckRedirect(checkForEmptyCart: true);
            if (results.IsNotEmpty()) return results;

            var msgs = new SystemMessages();

            var participationObject = SessionVariables.Cart.GetCartItems(msgs).FirstOrDefault()?.GetParticipationObject(msgs);
            var registration = PaymentViewBag(id, participationObject, msgs);

            // current registrant is already registered
            if (SessionVariables.CurrentRegistrant.IsRegisteredFor(participationObject, msgs) && registration.IsPaid(msgs))
                return RedirectToAction("select");

            return View();
        }

        [HttpGet]
        public ActionResult Payment(string id)
        {
            return HandleIsLoggedIn() ?? PaymentActionGet(id);
        }

        private ActionResult PaymentActionPost(string id, string stripetoken, string sourceId, string savepaymentinfo, string paymentoption, string couponCode)
        {
            var msgs = new SystemMessages();
            var participationObject = SessionVariables.Cart.GetCartItems(msgs).FirstOrDefault()?.GetParticipationObject(msgs);
            var coupon = new Coupon();

            if (couponCode.IsNotEmpty())
                coupon = SessionVariables.CurrentAccount.ValidateCoupon(participationObject, couponCode, msgs);

            if (sourceId.IsEmpty() && stripetoken.IsEmpty())
                msgs.Add("Please enter your payment information", MessageType.UserError);

            var registration = PaymentViewBag(id, participationObject, msgs);

            if (registration.IsAlreadyPaidFor(msgs))
                return RedirectToAction("thankyou", new { id = registration.Uid });

            if (msgs.HasNoErrors())
            {
                SubscriptionPlan paymentPlan = null;
                var billingAccount = participationObject.PaymentAccount;
                var payingAccount = SessionVariables.CurrentAccount;
                var convenienceFeePayer = billingAccount.IsConvenienceFeeTakenOutOfCost ? billingAccount : payingAccount;
                var creditCardFeePayer = billingAccount.IsCreditCardFeeTakenOutOfCost ? billingAccount : payingAccount;

                if (participationObject.UseStripeConnectedAccount)
                {
                    convenienceFeePayer = billingAccount;
                    creditCardFeePayer = billingAccount;
                }

                if (paymentoption.IsNotEmpty() && paymentoption.IsNumber())
                    paymentPlan = SubscriptionPlan.Load(paymentoption.ToInteger(), msgs);

                AccountService.ProcessRegistrationWithPayment(
                    checkoutObject: new CheckoutObject()
                    {
                        Coupon = coupon,
                        NumberOfSeats = 1,
                        ParticipationObject = participationObject,
                        PaymentPlan = paymentPlan,
                        SubscriptionPlan = participationObject.SubscriptionPlans.FirstOrDefault(),
                        Products = new List<Product>(),
                        PayingAccount = payingAccount,
                        BillingAccount = billingAccount,
                        Registration = registration,
                        StripeToken = stripetoken,
                        SourceId = sourceId,
                        SavePaymentInfo = savepaymentinfo.IfEmpty("0").ToBoolean(),
                        MasterRegistrant = SessionVariables.CurrentRegistrant,
                        PaymentType = PaymentType.CreditCard,
                        PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                        ConvenienceFeePayer = convenienceFeePayer,
                        CreditCardFeePayer = creditCardFeePayer
                    },
                    loggedInUser: SessionVariables.CurrentUser,
                    msgs: msgs);
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("thankyou", new { id = registration.Uid });

            return View();
        }

        [HttpPost]
        public ActionResult Payment(string id, string source, string stripetoken, string savepaymentinfo, string paymentoption, string coupon)
        {
            return HandleIsLoggedIn() ?? PaymentActionPost(id, stripetoken, source, savepaymentinfo, paymentoption, coupon);
        }

        private ActionResult MakePaymentActionGet(string id)
        {
            var msgs = new SystemMessages();
            var registration = Registration.LoadByUid(id.ToGuid(), msgs);

            if (registration.IsEmpty())
                return View("");

            // check if still owes money, load up variables
            if (!registration.IsPaid(msgs))
            {
                var participationObject = registration.GetParticipationObject(msgs);
                var currentRegistrant = registration.GetRegistrant(msgs).GetMasterRegistrant(msgs);
                var parentRegistrant = currentRegistrant.GetParentOrGuardian(msgs);
                var parentAccount = parentRegistrant.GetAccount(msgs);

                if (parentAccount.ID != SessionVariables.CurrentAccount.ID)
                    return Logout(Request.Url.ToString());

                SessionVariables.Cart = Cart.GetNewCart(loggedInUser: SessionVariables.CurrentUser, msgs: msgs);
                SessionVariables.Cart.AddCartItem(participationObject, msgs);
                SessionVariables.CurrentRegistrant = currentRegistrant;
                SessionVariables.RegistrationStartPageAccount = participationObject.OwnerAccount;

                return RedirectToAction("payment", new { id = registration.Uid });
            }

            return RedirectToAction("thankyou", new { id = registration.Uid });
        }

        [HttpGet]
        public ActionResult MakePayment(string id)
        {
            return HandleIsLoggedIn() ?? MakePaymentActionGet(id);
        }

        #endregion

        #region ThankYou

        private void ThankYouViewBag(string registrationUid, SystemMessages msgs)
        {            
            ResetRegistrationSession();

            var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);
            var registrant = registration.GetRegistrant(msgs);
            var masterRegistrant = registrant.GetMasterRegistrant(msgs);

            ViewBag.Registration = registration;
            ViewBag.Account = SessionVariables.RegistrationStartPageAccount;
            ViewBag.IsSelfRegistered = masterRegistrant.WebUserId == SessionVariables.CurrentUser.ID;
            ViewBag.Registrant = masterRegistrant;
            ViewBag.Messages = msgs;
        }

        private ActionResult ThankYouActionGet(string registrationUid)
        {
            var msgs = new SystemMessages();

            ThankYouViewBag(registrationUid, msgs);

            return View();
        }

        [HttpGet]
        public ActionResult ThankYou(string id)
        {
            return HandleIsLoggedIn() ?? ThankYouActionGet(id);
        }

        #endregion

        #region GetStartedStudent

        private List<FormQuestion> GetStartedStudentViewBag(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var personalContactGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = personalContactGroup.GetAllQuestions(msgs);

            questions = questions.Where(question =>
                    question.QuestionDataTypeId != QuestionDataType.RegistrantOrganization &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantGender &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantDateOfBirth &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantEthnicity &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantTShirtSize &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantHomePhone &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantWorkPhone
                )
                .ToList();

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantGrade:
                    case QuestionDataType.RegistrantEmail:
                        question.IsRequired = true;
                        break;
                }
            });

            ViewBag.Cart = SessionVariables.Cart;

            return questions;
        }

        [HttpGet]
        public ActionResult GetStartedStudent(string regaccount)
        {
            if (SessionVariables.CurrentUser.IsNotEmpty() && SessionVariables.CurrentAccount.AccountTypeId == AccountType.Registrant)
                return RedirectToAction("select");

            var msgs = new SystemMessages();
            var questions = GetStartedStudentViewBag(msgs);

            ViewBag.Messages = msgs;

            if (regaccount.IsGuid())
                ViewBag.RegistrationAccount = Account.LoadByUid(regaccount.ToGuid(), msgs);

            return View(questions.Select(question => new FormQuestionViewModel(question)).ToList());
        }

        [HttpPost]
        public ActionResult GetStartedStudent(string regaccount, string password1, string password2, string agree, string textnotifications)
        {
            var msgs = new SystemMessages();

            var questions = GetStartedStudentViewBag(msgs);

            questions.ForEach(question =>
            {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);
            });

            if (password1.IsEmpty())
                msgs.Add("Password 1 is required", MessageType.UserError);
            if (password1.Length < 8)
                msgs.Add("Password needs to have at least 8 characters", MessageType.UserError);
            if (password2.IsEmpty())
                msgs.Add("Password 2 is required", MessageType.UserError);
            if (password1 != password2)
                msgs.Add("Passwords must match", MessageType.UserError);
            if (!agree.IsBoolean())
                msgs.Add("You must agree to the terms of service", MessageType.UserError);

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
            {
                var participationObject = SessionVariables.Cart.GetCartItems(msgs).FirstOrDefault()?.GetParticipationObject(msgs);
                var firstName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName);
                var lastName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName);
                var email = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail);
                var phone = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone);                

                WebUser user = null;
                Account account = null;
                Registrant registrant = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // create user
                        user = WebUserService.CreateNewWithOptionalPhone(
                            firstName: firstName.Value,
                            lastName: lastName.Value,
                            email: email.Value.Trim(),
                            phone: phone.Value.Trim(),
                            password: password1,
                            isActive: true,
                            isRegistered: true,
                            isVerified: false,
                            allowTextMessages: textnotifications.IfBlank("0").ToBoolean(),
                            loggedInUser: null,
                            msgs: amsgs,
                            dc: adc);

                        // create account
                        account = user.AddNewRegistrantAccount(
                            userType: UserType.Student,
                            isActive: true,
                            isAfterSchool: SessionVariables.IsAfterSchool.Value,
                            loggedInUser: user,
                            msgs: amsgs,
                            dc: adc);

                        var location = participationObject?.Location;
                        if (location.IsNotEmpty() && location.GetHostAcount(amsgs, adc).AccountTypeId != AccountType.School)
                            location = null;

                        // create student registrant record
                        registrant = account.CreateRegistrant(
                            registrantOwner: user,
                            canBeAccessedBy: user.AsList(),
                            registrantType: RegistrantType.Individual,
                            isMaster: true,
                            isParentOrGuardian: false,
                            allowTextMessages: false,
                            schoolLocationId: location?.ID, // probably should have a school location id
                            profileStatus: RegistrantProfileStatus.Started,
                            loggedInUser: user,
                            msgs: amsgs,
                            dc: adc);

                        // add answers to student registrant record
                        qvms.ForEach(qvm =>
                        {
                            registrant.AddAnswer(
                                answerText: qvm.Value.IfEmpty("").Trim(),
                                question: qvm.Question,
                                loggedInUserId: user.ID.Value,
                                msgs: amsgs,
                                dc: adc);
                        });
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        SessionVariables.CurrentUser = user;
                        SessionVariables.CurrentAccount = account;
                        SessionVariables.AccountUserType = user.GetUserType(account);
                        SessionVariables.CurrentRegistrant = registrant;
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("inviteparent");
            ViewBag.Messages = msgs;
            return View(qvms);
        }

        #endregion

        #region InviteParent

        private List<FormQuestion> InviteParentViewBag(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var personalContactGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = personalContactGroup.GetAllQuestions(msgs);

            questions = questions.Where(question =>
                    question.QuestionDataTypeId != QuestionDataType.RegistrantOrganization &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantGrade &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantGender &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantDateOfBirth &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantEthnicity &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantTShirtSize &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantHomePhone &&
                    question.QuestionDataTypeId != QuestionDataType.RegistrantWorkPhone
                )
                .ToList();

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantEmail:
                        question.IsRequired = true;
                        break;
                }
            });

            ViewBag.Cart = SessionVariables.Cart;

            return questions;
        }

        [HttpGet]
        public ActionResult InviteParent()
        {
            var msgs = new SystemMessages();
            var questions = InviteParentViewBag(msgs);

            ViewBag.Messages = msgs;

            return View(questions.Select(question => new FormQuestionViewModel(question)).ToList());
        }

        [HttpPost]
        public ActionResult InviteParent(string textnotifications)
        {
            var msgs = new SystemMessages();

            var questions = InviteParentViewBag(msgs);
            // Get parent info from page
            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            var participationObject = SessionVariables.Cart.GetCartItems(msgs).FirstOrDefault()?.GetParticipationObject(msgs);
            var firstName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantFirstName);
            var lastName = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantLastName);
            var email = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantEmail);
            var phone = qvms.First(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.RegistrantMobilePhone);

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var location = participationObject?.Location;
                    var hostAccount = location?.GetHostAcount(amsgs, adc);
                    var locationString = "";

                    if (hostAccount.IsNotEmpty() && hostAccount.AccountTypeId != AccountType.School)
                        locationString = $" at {hostAccount.Name}";

                    // invite parent: 
                    var parentUser = SessionVariables.CurrentAccount.InviteUser(
                        firstName.Value,
                        lastName.Value,
                        email.Value.Trim(),
                        phone.Value.Trim(),
                        message: $"{SessionVariables.CurrentUser.FirstName} has signed up for an account on {AfterSchoolLibrary.Helpers.Utilities.AppName()}{locationString}. Please create your parent {AfterSchoolLibrary.Helpers.Utilities.AppName()} account to know and approve of activities your child requests to participate in.",
                        UserType.Owner,
                        SessionVariables.CurrentUser,
                        msgs);

                    // Create parent registrant
                    var parent = SessionVariables.CurrentAccount.CreateRegistrant(
                    registrantOwner: parentUser,
                    canBeAccessedBy: parentUser.AsList(),
                    registrantType: RegistrantType.Individual,
                    isMaster: true,
                    isParentOrGuardian: true,
                    allowTextMessages: textnotifications.IfBlank("").ToBoolean(),
                    schoolLocationId: null,
                    profileStatus: RegistrantProfileStatus.Started,
                    loggedInUser: SessionVariables.CurrentUser,
                    msgs: msgs,
                    dc: adc);

                    SessionVariables.CurrentRegistrant.AddUserWithAccess(
                        user: parentUser,
                        loggedInUser: SessionVariables.CurrentUser,
                        msgs: msgs,
                        dc: adc);

                    // add answers to student registrant record
                    qvms.ForEach(qvm =>
                    {
                        parent.AddAnswer(
                            answerText: qvm.Value.IfEmpty("").Trim(),
                            question: qvm.Question,
                            loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                            msgs: amsgs,
                            dc: adc);
                    });
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
            );

            if (msgs.HasNoErrors())
                return RedirectToAction("RegisterStudent");

            ViewBag.Messages = msgs;

            return View();
        }

        #endregion

        #region RegisterStudent

        [HttpGet]
        public ActionResult RegisterStudent()
        {
            var msgs = new SystemMessages();

            // check if parent has been invited 
            // if not redirect to parent invite page
            if (SessionVariables.CurrentRegistrant.GetParentOrGuardian(msgs).IsEmpty())
                return RedirectToAction("InviteParent");

            var participationObject = SessionVariables.Cart.GetCartItems().FirstOrDefault()?.GetParticipationObject();
            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.Messages = msgs;
            return View(participationObject);
        }

        [HttpPost]
        public ActionResult RegisterStudent(string agree)
        {
            var msgs = new SystemMessages();
            var participationObject = SessionVariables.Cart.GetCartItems().FirstOrDefault()?.GetParticipationObject();
            // need to see if there are any questions required by the form on the activity/activity group that haven't been collected on the main form

            if (agree.IsEmpty())
                msgs.Add("You must agree to participate", MessageType.UserError);

            Registration registration = null;

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // account gets copy / snapshot of master registrant record
                        // required in order to make sure registration data doesn't change as registrant changes information in master record.
                        var accountRegistrantCopy = SessionVariables.CurrentRegistrant.SubmitRecordToAccount(
                            ownerAccount: participationObject.OwnerAccount,
                            loggedInUser: SessionVariables.CurrentUser,
                            msgs: amsgs,
                            dc: adc);

                        // Create Registration
                        registration = accountRegistrantCopy.AddRegistration(
                            ownerAccountId: participationObject.OwnerAccount.ID.Value,
                            activityGroupId: participationObject.GetAnyRelatedActivityGroupId(),
                            accountActivityGroupId: participationObject.GetAccountActivityGroupId(),
                            periodId: participationObject.GetCurrentRegistrationPeriod(amsgs, adc)?.ID,
                            completionStatus: RegistrationCompletionStatus.RequiresParentSignature,
                            loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                            msgs: amsgs,
                            dc: adc);

                        if (msgs.HasNoErrors())
                            AccountService.ProcessStudentRegistration(
                                checkoutObject: new CheckoutObject()
                                {
                                    BillingAccount = participationObject.OwnerAccount,
                                    PayingAccount = SessionVariables.CurrentAccount,
                                    NumberOfSeats = 1,
                                    ParticipationObject = participationObject,
                                    Registration = registration,
                                    MasterRegistrant = SessionVariables.CurrentRegistrant
                                },
                                sendConfirmationEmail: true,
                                loggedInUser: SessionVariables.CurrentUser,
                                msgs: msgs, dc: adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("ThankYou", new { id = registration.Uid });

            ViewBag.Messages = msgs;
            ViewBag.Cart = SessionVariables.Cart;

            return View(participationObject);
        }

        #endregion

        #region Helpers

        private ActionResult CheckRedirect(bool checkForEmptyCart = false)
        {
            if (SessionVariables.CurrentUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return RedirectToAction(SessionVariables.RegistrationStartPageAccount.HomePageUrl);

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return Redirect(SessionVariables.RegistrationStartPageAccount.HomePageUrl);

            if (checkForEmptyCart && SessionVariables.Cart.IsEmpty())
                return RedirectToAction(SessionVariables.RegistrationStartPageAccount.HomePageUrl);

            return null;
        }

        private void ResetRegistrationSession()
        {
            SessionVariables.CurrentRegistrant = null;
            SessionVariables.Cart = null;
        }

        private ActionResult HandleIsLoggedIn()
        {
            if (SessionVariables.CurrentUser.IsEmpty() || SessionVariables.CurrentAccount.IsEmpty())
                return Logout(Request.Url.ToString(), true);

            if (SessionVariables.CurrentAccount.AccountTypeId != AccountType.Registrant)
                return Logout(Request.Url.ToString(), true);

            return null;
        }

        private Form GetAdministratorAccountForm(SystemMessages msgs)
        {
            var administratorAccount = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs).First();
            return administratorAccount.GetDefaultForm(msgs);
        }

        #endregion
    }
}