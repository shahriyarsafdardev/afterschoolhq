using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfterSchool.Areas.User.Controllers
{
    public class FamilyController : BaseLoggedInController
    {
        public FamilyController() : base(new List<AccountType>() { AccountType.Registrant })
        {

        }

        // GET: User/Family
        public ActionResult Index()
        {
            var msgs = new SystemMessages();
            var registrants = SessionVariables.CurrentUser.GetAllRegistrants(msgs);

            return View(registrants);
        }

        #region StudentPersonal

        private List<FormQuestion> GetStudentPersonalQuestions(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var studentQuestionGroup = form.GetQuestionGroups(msgs)
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

            return questions;
        }

        [HttpGet]
        public ActionResult EditStudentPersonal(string id, string resetgrade)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");

                var questions = GetStudentPersonalQuestions(msgs);

                var values = new NameValueCollection();
                questions.ForEach(question => {
                    values.Add(question.ToFormQuestionViewModel().Id, registrant.GetAnswer(question.QuestionDataTypeId)?.Text);
                });

                ViewBag.QuestionViewModels = questions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
                ViewBag.School = registrant.GetSchoolLocation(msgs);
            }

            ViewBag.Messages = msgs;
            ViewBag.ResetGrade = resetgrade.IsNotEmpty().ToString().ToLower();

            return View(registrant);
        }

        // using FormCollection instead of location parameters, as location parameters where not binding properly
        // and we also need location parameters like decimal latitude and decimal longitude to be nullable, as placeid can be empty
        // when user checks 'schoolnotfound', so formcollection is used.
        [HttpPost]
        public ActionResult EditStudentPersonal(string id, string reg, string student, string returnUrl, FormCollection formCollection)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");

                var questions = GetStudentPersonalQuestions(msgs);

                // Validation
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
                    Database.ExecuteTransaction
                    (
                        action: (SystemMessages amsgs, DatabaseConnection adc) =>
                        {
                            Location school = null;

                            if (formCollection["placeid"].IsNotEmpty())
                                school = LocationService.CreateOrUpdateFromGoogle(formCollection["locationname"], formCollection["streetnumber"], formCollection["streetname"] , formCollection["address2"] , formCollection["neighborhood"] , formCollection["city"] , formCollection["county"], formCollection["state"], formCollection["postalcode"], formCollection["country"], formCollection["placeid"], Convert.ToDecimal( formCollection["latitude"]), Convert.ToDecimal(formCollection["longitude"]), SessionVariables.CurrentUser, amsgs, adc);

                            registrant.SchoolLocationId = school.IsNotEmpty() ? school.ID : new int?();
                            registrant.LastModifiedBy = SessionVariables.CurrentUser.ID;
                            registrant.LastModifiedDate = DateTime.UtcNow;
                            amsgs.AddRange(registrant.Save(adc));

                            qvms.ForEach(qvm =>
                            {
                                registrant.AddOrUpdateAnswer(
                                    answerText: qvm.Value.IfEmpty("").Trim(),
                                    question: qvm.Question,
                                    loggedInUser: SessionVariables.CurrentUser,
                                    msgs: amsgs,
                                    dc: adc);
                            });
                        },
                        onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                        onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                        onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                    );
                }

                ViewBag.QuestionViewModels = qvms;
                ViewBag.School = registrant.GetSchoolLocation(msgs);
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("EditHealth", new { id = registrant.Uid, reg, student, returnUrl });

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        #endregion

        #region Health

        private List<FormQuestion> GetHealthQuestions(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var questionGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.HealthInformation);
            var questions = questionGroup.GetAllQuestions(msgs);

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    default:
                        question.IsRequired = false;
                        break;
                }
            });

            return questions;
        }

        [HttpGet]
        public ActionResult EditHealth(string id)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");

                var questions = GetHealthQuestions(msgs);

                var values = new NameValueCollection();
                questions.ForEach(question => {
                    values.Add(question.ToFormQuestionViewModel().Id, registrant.GetAnswer(question.QuestionDataTypeId)?.Text);
                });

                ViewBag.QuestionViewModels = questions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
            }

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        [HttpPost]
        public ActionResult EditHealth(string id, string reg, string student, string returnUrl)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");

                var questions = GetHealthQuestions(msgs);

                // Validation
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
                            registrant.SetProfileAsCompleted(SessionVariables.CurrentUser, amsgs, adc);

                            qvms.ForEach(qvm =>
                            {
                                registrant.AddOrUpdateAnswer(
                                    answerText: qvm.Value.IfEmpty("").Trim(),
                                    question: qvm.Question,
                                    loggedInUser: SessionVariables.CurrentUser,
                                    msgs: amsgs,
                                    dc: adc);
                            });
                        },
                        onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                        onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                        onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                    );
                }

                ViewBag.QuestionViewModels = qvms;
            }


            if (returnUrl.IsNotEmpty())
                return Redirect(returnUrl);

            if (student.IsGuid())
            {
                SessionVariables.CurrentRegistrant = registrant;
                return Redirect("/account/register");
            }

            if (reg.IsGuid())
                return Redirect(Registration.LoadByUid(reg.ToGuid()).GetParentalConsentUrl);

            if (msgs.HasNoErrors())
                return RedirectToAction("EditProfileDone", new { id = registrant.Uid, reg, student });

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        #endregion

        #region Personal

        private List<FormQuestion> GetPeronalQuestions(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var questionGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation);
            var questions = questionGroup.GetAllQuestions(msgs);

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

            return questions;
        }

        [HttpGet]
        public ActionResult EditPersonal(string id)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");

                var questions = GetPeronalQuestions(msgs);

                var values = new NameValueCollection();
                questions.ForEach(question => {
                    values.Add(question.ToFormQuestionViewModel().Id, registrant.GetAnswer(question.QuestionDataTypeId)?.Text);
                });

                ViewBag.QuestionViewModels = questions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
            }

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        [HttpPost]
        public ActionResult EditPersonal(string id, string reg, string student)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");

                var questions = GetPeronalQuestions(msgs);

                // Validation
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

                var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

                if (msgs.HasNoErrors())
                {
                    Database.ExecuteTransaction
                    (
                        action: (SystemMessages amsgs, DatabaseConnection adc) =>
                        {
                            qvms.ForEach(qvm =>
                            {
                                registrant.AddOrUpdateAnswer(
                                    answerText: qvm.Value.IfEmpty("").Trim(),
                                    question: qvm.Question,
                                    loggedInUser: SessionVariables.CurrentUser,
                                    msgs: amsgs,
                                    dc: adc);
                            });
                            
                            // copy parent 1 info over to the student
                            var parent1Map = new Dictionary<QuestionDataType, QuestionDataType>()
                            {
                                { QuestionDataType.RegistrantFirstName,  QuestionDataType.Parent1FirstName },
                                { QuestionDataType.RegistrantLastName,  QuestionDataType.Parent1LastName },
                                { QuestionDataType.RegistrantEmail,  QuestionDataType.Parent1Email },
                                { QuestionDataType.RegistrantHomePhone,  QuestionDataType.Parent1HomePhone },
                                { QuestionDataType.RegistrantMobilePhone,  QuestionDataType.Parent1MobilePhone },
                                { QuestionDataType.RegistrantWorkPhone,  QuestionDataType.Parent1WorkPhone },
                            };

                            var children = SessionVariables.CurrentAccount.GetChildrenRegistrants(msgs, adc);

                            children.ForEach(child =>
                            {
                                foreach (var key in parent1Map.Keys)
                                {
                                    child.MapAndAddAnswer(
                                       questions,
                                       registrant,
                                       key,
                                       parent1Map[key],
                                       SessionVariables.CurrentUser,
                                       amsgs,
                                       adc);
                                }
                            });

                        },
                        onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                        onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                        onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                    );
                }

                ViewBag.QuestionViewModels = qvms;
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("EditHomeAddress", new { id = registrant.Uid, reg, student });

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        #endregion

        #region HomeAddress

        private List<FormQuestion> GetHomeAddressQuestions(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var questionGroup = form.GetQuestionGroups(msgs)
                .First(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.HomeAddress);
            var questions = questionGroup.GetAllQuestions(msgs);

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

            return questions;
        }

        [HttpGet]
        public ActionResult EditHomeAddress(string id)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");

                var questions = GetHomeAddressQuestions(msgs);

                var values = new NameValueCollection();
                questions.ForEach(question => {
                    values.Add(question.ToFormQuestionViewModel().Id, registrant.GetAnswer(question.QuestionDataTypeId)?.Text);
                });

                ViewBag.QuestionViewModels = questions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
            }

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        [HttpPost]
        public ActionResult EditHomeAddress(string id, string reg, string student)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");

                var questions = GetHomeAddressQuestions(msgs);

                // Validation
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
                            qvms.ForEach(qvm =>
                            {
                                registrant.AddOrUpdateAnswer(
                                    answerText: qvm.Value.IfEmpty("").Trim(),
                                    question: qvm.Question,
                                    loggedInUser: SessionVariables.CurrentUser,
                                    msgs: amsgs,
                                    dc: adc);
                            });

                            UpdateChildrenRegistrants(registrant, QuestionGroupDataType.HomeAddress.AsList(), amsgs, adc);
                        },
                        onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                        onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                        onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                    );
                }

                ViewBag.QuestionViewModels = qvms;
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("EditEmergencyContact", new { id = registrant.Uid, reg, student });

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        #endregion

        #region EmergencyContacts

        private List<FormQuestion> GetEmergencyContactQuestions(SystemMessages msgs)
        {
            var form = GetAdministratorAccountForm(msgs);
            var emergencyContactsQuestionGroups = form.GetQuestionGroups(msgs)
                .Where(group =>
                    group.QuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact1 ||
                    group.QuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact2 ||
                    group.QuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact3)
                .ToList();
            var questions = new List<FormQuestion>();

            emergencyContactsQuestionGroups.ForEach(group => questions.AddRange(group.GetAllQuestions(msgs)));

            return questions;
        }

        [HttpGet]
        public ActionResult EditEmergencyContact(string id)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");

                var questions = GetEmergencyContactQuestions(msgs);

                var values = new NameValueCollection();
                questions.ForEach(question => {
                    values.Add(question.ToFormQuestionViewModel().Id, registrant.GetAnswer(question.QuestionDataTypeId)?.Text);
                });

                ViewBag.QuestionViewModels = questions.Select(question => question.ToFormQuestionViewModel(values)).ToList();
            }

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        [HttpPost]
        public ActionResult EditEmergencyContact(string id, string reg, string student)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");

                var questions = GetEmergencyContactQuestions(msgs);
                var phones = new List<string>();
                var firstNames = new List<string>();
                var lastNames = new List<string>();
                var primaryPhones = new List<string>();

                // Validation
                questions.ForEach(question =>
                {
                    var answer = Request[question.ToFormQuestionViewModel().Name];

                    if (question.IsRequired && answer.IsEmpty())
                        msgs.Add($"{question.Text} is required", MessageType.UserError);

                    // store first names
                    if (question.QuestionDataTypeId.In(new List<QuestionDataType>() { QuestionDataType.EmergencyContact1FirstName, QuestionDataType.EmergencyContact2FirstName, QuestionDataType.EmergencyContact3FirstName }))
                        firstNames.Add(answer);

                    // store last names
                    if (question.QuestionDataTypeId.In(new List<QuestionDataType>() { QuestionDataType.EmergencyContact1LastName, QuestionDataType.EmergencyContact2LastName, QuestionDataType.EmergencyContact3LastName }))
                        lastNames.Add(answer);

                    // store primary phone numbers
                    if (question.QuestionDataTypeId.In(new List<QuestionDataType>() { QuestionDataType.EmergencyContact1Phone, QuestionDataType.EmergencyContact2Phone, QuestionDataType.EmergencyContact3Phone }))
                        primaryPhones.Add(answer);

                    switch (question.QuestionDataTypeId)
                    {
                        case QuestionDataType.EmergencyContact1Phone:
                        case QuestionDataType.EmergencyContact1Phone2:
                        case QuestionDataType.EmergencyContact1Phone3:
                        case QuestionDataType.EmergencyContact2Phone:
                        case QuestionDataType.EmergencyContact2Phone2:
                        case QuestionDataType.EmergencyContact2Phone3:
                        case QuestionDataType.EmergencyContact3Phone:
                        case QuestionDataType.EmergencyContact3Phone2:
                        case QuestionDataType.EmergencyContact3Phone3:
                            if (answer.IsNotEmpty() && !answer.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                                msgs.Add($"{question.FormQuestionGroupQuestionGroupDataTypeId.ToString().SplitByUpperCase()} {question.Text} must be a valid phone number", MessageType.UserError);

                            if (answer.IsNotEmpty())
                                phones.Add(answer);

                            break;
                    }
                });

                if ((phones.Count == 2 && phones[0] == phones[1]) || 
                    (phones.Count == 3 && (phones[0] == phones[1] || phones[0] == phones[2] || phones[1] == phones[2])))
                    msgs.Add("Emergency phone numbers should not be the same", MessageType.UserError);

                ValidateEmergencyContacts(firstName: firstNames[1], lastName: lastNames[1], phone: primaryPhones[1], ecNumber: 2, msgs: msgs);
                ValidateEmergencyContacts(firstName: firstNames[2], lastName: lastNames[2], phone: primaryPhones[2], ecNumber: 3, msgs: msgs);

                var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

                if (msgs.HasNoErrors())
                {
                    Database.ExecuteTransaction
                    (
                        action: (SystemMessages amsgs, DatabaseConnection adc) =>
                        {
                            // mark parent profile as completed since they've went through all the steps
                            registrant.SetProfileAsCompleted(SessionVariables.CurrentUser, amsgs, adc);

                            qvms.ForEach(qvm =>
                            {
                                registrant.AddOrUpdateAnswer(
                                    answerText: qvm.Value.IfEmpty("").Trim(),
                                    question: qvm.Question,
                                    loggedInUser: SessionVariables.CurrentUser,
                                    msgs: amsgs,
                                    dc: adc);
                            });

                            UpdateChildrenRegistrants(
                                registrant, 
                                new List<QuestionGroupDataType>()
                                {
                                    QuestionGroupDataType.EmergencyContact1,
                                    QuestionGroupDataType.EmergencyContact2,
                                    QuestionGroupDataType.EmergencyContact3
                                }, 
                                amsgs, 
                                adc);
                        },
                        onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                        onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                        onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                    );
                }

                ViewBag.QuestionViewModels = qvms;
            }

            if (student.IsGuid())
            {
                var regMasterRegistrant = Registrant.LoadByUid(student.ToGuid(), msgs);
                if (!regMasterRegistrant.IsProfileCompleted)
                    return RedirectToAction("EditStudentPersonal", new { id = regMasterRegistrant.Uid, reg, student });

                return Redirect("/account/register");
            }

            if (reg.IsGuid())
            {
                var registration = Registration.LoadByUid(reg.ToGuid());
                var regMasterRegistrant = registration.GetRegistrant().GetMasterRegistrant();

                if (!regMasterRegistrant.IsProfileCompleted)
                    return RedirectToAction("EditStudentPersonal", new { id = regMasterRegistrant.Uid, reg = reg });

                return Redirect(registration.GetParentalConsentUrl);
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("EditProfileDone", new { id = registrant.Uid });

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        public static SystemMessages ValidateEmergencyContacts(string firstName, string lastName, string phone, int ecNumber, SystemMessages msgs = null)
        {
            if (firstName.IsEmpty() && (lastName.IsNotEmpty() || phone.IsNotEmpty()))
                msgs.Add($"First name for contact {ecNumber} is required.", MessageType.UserError);

            if (lastName.IsEmpty() && (firstName.IsNotEmpty() || phone.IsNotEmpty()))
                msgs.Add($"Last name for contact {ecNumber} is required.", MessageType.UserError);

            if (phone.IsEmpty() && (firstName.IsNotEmpty() || lastName.IsNotEmpty()))
                msgs.Add($"Primary phone number for contact {ecNumber} is required.", MessageType.UserError);

            return msgs;
        }

        #endregion

        [HttpGet]
        public ActionResult EditProfileDone(string id)
        {
            var msgs = new SystemMessages();
            Registrant registrant = null;

            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid(), msgs);

                if (!SessionVariables.CurrentUser.HasAccessToRegistrant(registrant))
                    return RedirectToAction("index");
            }

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        private void UpdateChildrenRegistrants(Registrant registrant, List<QuestionGroupDataType> questionGroupDataTypes, SystemMessages msgs, DatabaseConnection dc)
        {
            // if this is the parent and owner of this account
            if (registrant.IsParentOrGuardian && registrant.GetOwner(msgs, dc)?.ID == SessionVariables.CurrentAccount.GetAccountOwner(msgs, dc).ID)
            {
                var students = SessionVariables.CurrentAccount.GetChildrenRegistrants(msgs, dc);

                // copy parent answers to students
                registrant.GetAnswers(msgs, dc)
                    .Where(answer => answer.FormQuestionGroupQuestionGroupDataTypeId.In(questionGroupDataTypes))
                    .ToList()
                    .ForEach(answer =>
                    {
                        students.ForEach(student => {
                            student.AddOrUpdateAnswer(
                                answerText: answer.Text.IfEmpty("").Trim(),
                                question: answer.ToFormQuestion(msgs: msgs, dc: dc),
                                    loggedInUser: SessionVariables.CurrentUser,
                                msgs: msgs,
                                dc: dc);
                        });
                    });
            }
        }

        private Form GetAdministratorAccountForm(SystemMessages msgs)
        {
            var administratorAccount = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs).First();
            return administratorAccount.GetDefaultForm(msgs);
        }
    }
}