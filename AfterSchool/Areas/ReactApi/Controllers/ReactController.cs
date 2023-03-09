using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Controllers;
using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchool.Areas.ReactApi.Controllers
{
    public class ReactController : BaseController
    {
        [HttpGet]
        public ActionResult GetCurrentUser()
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;

                if (currentUser.IsEmpty())
                {
                    msgs.Add("User Not Found" , MessageType.ApiError);
                    throw new Exception("Unauthorized");
                }

                return Json(
                new ApiResponse(msgs,
                    new WebUserDto(currentUser)),
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs,null),
                    JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpGet]
        public ActionResult GetForm()
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                var currentAccount = SessionVariables.CurrentAccount;
                var currentUserRegistrant = Registrant.LoadByWebUserIdByIsDeleted(currentUser.ID.Value, false, msgs, null).FirstOrDefault();
                var currentRegistrant = SessionVariables.CurrentRegistrant;
                Registrant parentRegistrant = null;

                if (currentRegistrant.IsStudent)
                    parentRegistrant = currentRegistrant.GetParentOrGuardian(msgs);

                // var currentRegistrant = currentUser.GetAllRegistrants().FirstOrDefault();
                // var studentRegistrant = currentAccount.GetStudentRegistrants(msgs).FirstOrDefault();
                
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");
                
                var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
                var activity = participationObject.GetActivity();
                var questions = participationObject.Form.GetAllQuestions(msgs);
                var questionGroups = participationObject.Form.GetQuestionGroups(msgs);
                
                if (currentRegistrant.IsAdult)
                {
                    questionGroups = questionGroups.Where(group =>
                        group.QuestionGroupDataTypeId.NotIn(FormConstants.QuestionsGroupToNotIncludeInAdultPanel())
                    ).ToList();
                }
                
                var formQuestionDtoList = new List<FormQuestionDto>();
                var formQuestionAnswerDtoList = new List<FormQuestionAnswerDto>();
                var formQuestionGroupDtoList = new List<FormQuestionGroupDto>();

                foreach (var question in questions)
                {
                    formQuestionDtoList.Add(
                        new FormQuestionDto()
                        {
                             ID = question.ID.Value,
                             FormQuestionGroupId = question.FormQuestionGroupId,
                             Text = question.Text,
                             Description = question.Description,
                             Options = question.Options,
                             IsRequired = question.IsRequired,
                             DisplayOrder = question.DisplayOrder,
                             IsActive = question.IsActive,
                             IsDeleted = question.IsDeleted,
                             QuestionTypeId = question.QuestionTypeId,
                             QuestionDataTypeId = question.QuestionDataTypeId,
                             FormQuestionGroupQuestionGroupDataTypeId = question.FormQuestionGroupQuestionGroupDataTypeId
                        }
                    );
                }
                
                foreach (var questionGroup in questionGroups)
                {
                    formQuestionGroupDtoList.Add(
                        new FormQuestionGroupDto()
                        {
                            FormId = questionGroup.FormId,
                            Name = questionGroup.Name,
                            DisplayOrder = questionGroup.DisplayOrder,
                            QuestionGroupDataTypeId = questionGroup.QuestionGroupDataTypeId,
                        }
                    );
                }
                
                foreach (var questionAnswer in FormQuestionAnswer.LoadByRegistrantId(currentRegistrant.ID.Value))
                {
                    formQuestionAnswerDtoList.Add(
                        new FormQuestionAnswerDto()
                        {
                            FormQuestionId = questionAnswer.FormQuestionId,
                            QuestionDataTypeId = (int)questionAnswer.QuestionDataTypeId,
                            QuestionText = questionAnswer.QuestionText,
                            Text = questionAnswer.Text
                        }
                    );
                }

                dynamic response = new
                {
                    FormDto = new FormDto(formQuestionDtoList, formQuestionGroupDtoList, formQuestionAnswerDtoList),
                    activityDto = new
                    {
                        Name = activity.Name,
                        Organization = activity.AccountName,
                        IsFree = participationObject.IsFree,
                        RegistrantType = currentUserRegistrant.PersonTypeId.ToString(),
                        FindAnotherActivityUrl = participationObject.OwnerAccount.ShortHomePageUrl,
                        ActivityUrl = activity.ShortHomePageUrl,
                        RegisterForNewChildUrl = participationObject.GetRegistrationPageUrl()
                    }
                };
                
                return Json(
                    new ApiResponse(msgs,response),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs,null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetNewChildForm()
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized");
                
                var currentAccount = SessionVariables.CurrentAccount;
                var parentRegistrant = currentAccount.GetFirstParentOrGuardianRegistrant(msgs);
                var parent2Registrant = currentAccount.GetSecondParentOrGuardianRegistrant(msgs);
                var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
                var activity = participationObject.GetActivity();
                var questions = participationObject.Form.GetAllQuestions(msgs);
                var questionGroups = participationObject.Form.GetQuestionGroups(msgs);
                Registrant studentReg = null;
                
                Database.ExecuteTransaction
                (
                    (amsgs, adc) =>
                    {
                        studentReg = currentAccount.CreateParentOrStudentRegistrant(
                            registrantOwner: currentUser,
                            canBeAccessedBy: currentUser.AsList(),
                            registrantType: RegistrantType.Individual,
                            isMaster: true,
                            isParentOrGuardian: false,
                            allowTextMessages: false,
                            schoolLocationId: null,
                            profileStatus: RegistrantProfileStatus.Started,
                            loggedInUser: currentUser,
                            msgs: amsgs,
                            dc : adc);
                        
                        if(parentRegistrant.IsNotEmpty())
                            RegistrationCommon.MapParent1AnswersToStudent(parentRegistrant, studentReg, participationObject.Form.GetAllQuestions(msgs), currentUser, amsgs, adc);
                
                        if(parent2Registrant.IsNotEmpty())
                            RegistrationCommon.MapParent2AnswersToStudent(parent2Registrant, studentReg, participationObject.Form.GetAllQuestions(msgs), currentUser, amsgs, adc);
                    },
                    smsgs =>
                    {
                        msgs.AddRange(smsgs);

                        SessionVariables.CurrentRegistrant = studentReg;
                    },
                    fmsgs => msgs.AddRange(fmsgs),
                    emsgs => throw new Exception(emsgs.ToString())
                );
                
                var formQuestionDtoList = new List<FormQuestionDto>();
                var formQuestionAnswerDtoList = new List<FormQuestionAnswerDto>();
                var formQuestionGroupDtoList = new List<FormQuestionGroupDto>();

                foreach (var question in questions)
                {
                    formQuestionDtoList.Add(
                        new FormQuestionDto()
                        {
                             ID = question.ID.Value,
                             FormQuestionGroupId = question.FormQuestionGroupId,
                             Text = question.Text,
                             Description = question.Description,
                             Options = question.Options,
                             IsRequired = question.IsRequired,
                             DisplayOrder = question.DisplayOrder,
                             IsActive = question.IsActive,
                             IsDeleted = question.IsDeleted,
                             QuestionTypeId = question.QuestionTypeId,
                             QuestionDataTypeId = question.QuestionDataTypeId,
                             FormQuestionGroupQuestionGroupDataTypeId = question.FormQuestionGroupQuestionGroupDataTypeId
                        }
                    );
                }
                
                foreach (var questionGroup in questionGroups)
                {
                    formQuestionGroupDtoList.Add(
                        new FormQuestionGroupDto()
                        {
                            FormId = questionGroup.FormId,
                            Name = questionGroup.Name,
                            DisplayOrder = questionGroup.DisplayOrder,
                            QuestionGroupDataTypeId = questionGroup.QuestionGroupDataTypeId,
                        }
                    );
                }
                
                foreach (var questionAnswer in FormQuestionAnswer.LoadByRegistrantId(studentReg.ID.Value))
                {
                    formQuestionAnswerDtoList.Add(
                        new FormQuestionAnswerDto()
                        {
                            FormQuestionId = questionAnswer.FormQuestionId,
                            QuestionDataTypeId = (int)questionAnswer.QuestionDataTypeId,
                            QuestionText = questionAnswer.QuestionText,
                            Text = questionAnswer.Text
                        }
                    );
                }

                dynamic response = new
                {
                    FormDto = new FormDto(formQuestionDtoList, formQuestionGroupDtoList, formQuestionAnswerDtoList),
                    activityDto = new
                    {
                        Name = activity.Name,
                        Organization = activity.AccountName,
                        IsFree = participationObject.IsFree,
                        RegistrantType = RegistrantPersonType.Parent
                    }
                };
                
                return Json(
                    new ApiResponse(msgs,response),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs,null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddUpdateAnswers(QuestionAnswerResponse json)
        {
            var msgs = new SystemMessages();

            try
            {
                var currentUser = SessionVariables.CurrentUser;
                var currentAccount = SessionVariables.CurrentAccount;
                var currentRegistrant = SessionVariables.CurrentRegistrant;
                var currentUserPersonType = currentUser.GetMasterParentOrStudentOrAdultRegistrant(msgs).PersonTypeId;
                var parentRegistrant = currentAccount.GetFirstParentOrGuardianRegistrant(msgs);
                var parent2Registrant = currentAccount.GetSecondParentOrGuardianRegistrant(msgs);

                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized");

                //var group = Regex.Match(json.groupid, @"\d+").Value;
                var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
                var questions = participationObject.Form.GetAllQuestions(msgs);
                var collection = HttpUtility.ParseQueryString(json.answers);
                var questionGroups = participationObject.Form.GetQuestionGroups(msgs);

                var qvms = questions.Select(question => question.ToFormQuestionViewModel(collection)).ToList();
                
                qvms.Where(qvm => qvm.Question.FormQuestionGroupId == json.groupid.ToInteger()).ToList().ForEach(qvm =>
                {
                    // if (qvm.Question.IsRequired && qvm.Value.IsEmpty())
                    //     msgs.Add($"{qvm.Question.Text} is required", MessageType.UserError);

                    switch (qvm.Question.QuestionDataTypeId)
                    {
                        case QuestionDataType.RegistrantMobilePhone:
                        case QuestionDataType.RegistrantDoctorPhone:
                        case QuestionDataType.RegistrantHomePhone:
                        case QuestionDataType.RegistrantWorkPhone:
                        case QuestionDataType.RegistrantPhone1:
                        case QuestionDataType.RegistrantPhone2:
                        case QuestionDataType.Parent1Phone1:
                        case QuestionDataType.Parent2Phone1:
                        case QuestionDataType.Parent1Phone2:
                        case QuestionDataType.Parent2Phone2:
                        case QuestionDataType.Parent1MobilePhone:
                        case QuestionDataType.Parent2MobilePhone:
                        case QuestionDataType.Parent1WorkPhone:
                        case QuestionDataType.Parent2WorkPhone:
                        case QuestionDataType.Parent1HomePhone:
                        case QuestionDataType.Parent2HomePhone:
                        case QuestionDataType.EmergencyContact1Phone:
                        case QuestionDataType.EmergencyContact2Phone:
                        case QuestionDataType.EmergencyContact3Phone:
                        case QuestionDataType.EmergencyContact1Phone2:
                        case QuestionDataType.EmergencyContact2Phone2:
                        case QuestionDataType.EmergencyContact3Phone2:
                        case QuestionDataType.EmergencyContact1Phone3:
                        case QuestionDataType.EmergencyContact2Phone3:
                        case QuestionDataType.EmergencyContact3Phone3:
                            if (qvm.Value.IsNotEmpty() && !qvm.Value.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                                msgs.Add($"{qvm.Question.Text} must be a valid phone number", MessageType.UserError);
                            break;
                        case QuestionDataType.RegistrantEmail:
                        case QuestionDataType.Parent1Email:
                        case QuestionDataType.Parent2Email:
                            if (qvm.Value.IsNotEmpty() && !qvm.Value.IsRegExMatch(BasicLibrary.Utilities.EmailRegEx))
                                msgs.Add($"{qvm.Question.Text} must be a valid email", MessageType.UserError);
                            break;
                    }
                });
                
                var parent1FirstName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent1FirstName).Value;
                var parent1LastName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent1LastName).Value;
                var parent1MiddleName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent1MiddleName)?.Value;
                var parent1Phone = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent1MobilePhone).Value;
                var parent1Email = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent1Email).Value;

                var parent2FirstName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent2FirstName).Value;
                var parent2LastName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent2LastName).Value;
                var parent2MiddleName = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent2MiddleName)?.Value;
                var parent2Phone = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent2MobilePhone).Value;
                var parent2Email = qvms.Find(qvm => qvm.Question.QuestionDataTypeId == QuestionDataType.Parent2Email).Value;

                if (json.schoolLocationID.IsNotEmpty() && currentRegistrant.IsStudent)
                    currentRegistrant.SchoolLocationId = json.schoolLocationID;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {                
                        if (currentUserPersonType == RegistrantPersonType.Parent && json.newChild != 1)
                        {
                            if (json.groupid.ToInteger() == (int) QuestionGroupDataType.Parent1Information && parentRegistrant.IsEmpty())
                            {
                                var parent1Reg = currentAccount.CreateParentOrStudentRegistrant(
                                    registrantOwner: currentUser,
                                    canBeAccessedBy: currentUser.AsList(),
                                    registrantType: RegistrantType.Individual,
                                    isMaster: true,
                                    isParentOrGuardian: true,
                                    allowTextMessages: false,
                                    schoolLocationId: null,
                                    profileStatus: RegistrantProfileStatus.Started,
                                    loggedInUser: currentUser,
                                    msgs: amsgs,
                                    dc: adc);

                                parent1Reg.FirstName = parent1FirstName;
                                parent1Reg.LastName = parent1LastName;
                                parent1Reg.MiddleName = parent1MiddleName;
                                parent1Reg.Email = parent1Email;
                                parent1Reg.Phone = parent1Phone;

                                amsgs.AddRange(parent1Reg.Save(adc));
                            }
                    
                            if (json.groupid.ToInteger() == (int) QuestionGroupDataType.Parent2Information && parent2Registrant.IsEmpty())
                            {
                                var parent2Reg = currentAccount.CreateParentOrStudentRegistrant(
                                    registrantOwner: null,
                                    canBeAccessedBy: currentUser.AsList(),
                                    registrantType: RegistrantType.Individual,
                                    isMaster: true,
                                    isParentOrGuardian: true,
                                    allowTextMessages: true,
                                    schoolLocationId: null,
                                    profileStatus: RegistrantProfileStatus.Started,
                                    loggedInUser: currentUser,
                                    msgs: amsgs,
                                    dc: adc);

                                parent2Reg.FirstName = parent2FirstName;
                                parent2Reg.LastName = parent2LastName;
                                parent2Reg.MiddleName = parent2MiddleName;
                                parent2Reg.Email = parent2Email;
                                parent2Reg.Phone = parent2Phone;

                                amsgs.AddRange(parent2Reg.Save(adc));
                            }
                        }
                
                        qvms.ForEach(qvm =>
                        {                            
                            currentRegistrant.AddOrUpdateAnswer(
                                qvm.Value.IfEmpty("").Trim(),
                                qvm.Question,
                                currentUser,
                                amsgs,
                                adc);
                        });

                        if (json.groupid.ToInteger() == -1)
                        {
                            currentRegistrant.SetProfileAsCompleted(currentUser, amsgs, adc);

                            if (json.newChild != -1)
                            {
                                if(currentUserPersonType == RegistrantPersonType.Parent)
                                {                        
                                    if(parentRegistrant.IsNotEmpty())
                                        RegistrationCommon.MapStudentAnswersToParent1(currentRegistrant, parentRegistrant, questions, currentUser, amsgs, adc);

                                    if (parent2Registrant.IsNotEmpty())
                                        RegistrationCommon.MapStudentAnswersToParent2(currentRegistrant, parent2Registrant, questions, currentUser, amsgs, adc);
                                }                  
                            }
                        }


                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );

                return Json(
                    new ApiResponse(msgs,null),
                    JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs,null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Register(QuestionAnswerResponse json)
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                var currentAccount = SessionVariables.CurrentAccount;
                var currentRegistrant = SessionVariables.CurrentRegistrant;
                
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized");

                if (json.esignature.IsEmpty())
                    msgs.Add("E-signature is required", MessageType.UserError);                

                var participationObject = RegistrationCommon.GetParticipationObjectFromCart();
                var questions = participationObject.Form.GetAllQuestions(msgs);
                NameValueCollection collection = HttpUtility.ParseQueryString(json.answers);

                var qvms = questions.Select(question => question.ToFormQuestionViewModel(collection)).ToList();
                var registration = RegistrationCommon.CreateRegistration(participationObject, qvms, json.esignature, Request, msgs);
                
                if(participationObject.IsFree)
                {
                    var selectedDates = RegistrationCommon.GetSelectedDatesFromCart();
                    var datesToRemove = new List<DateTime>();

                    selectedDates?.ForEach(date =>
                    {
                        if (currentRegistrant.IsReservedForDate(participationObject, date, null))
                            datesToRemove.Add(date);
                    });

                    datesToRemove.ForEach(date => selectedDates.Remove(date));

                    if (msgs.HasNoErrors())
                        AccountService.ProcessRegistrationWithoutPayment(
                            new CheckoutObject
                            {
                                BillingAccount = participationObject.OwnerAccount,
                                PayingAccount = currentAccount,
                                NumberOfSeats = 1,
                                ParticipationObject = participationObject,
                                Registration = registration,
                                MasterRegistrant = currentRegistrant,
                                SubscriptionPlan = participationObject.SubscriptionPlans.FirstOrDefault(),
                                SelectedDates = selectedDates,
                            },
                            true,
                            SessionVariables.CurrentUser,
                            msgs);
                }
                
                return Json(
                    new ApiResponse(msgs,registration.Uid),
                    JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs,null),
                    JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult CreateOrUpdateFromGoogle(LocationDto json)
        {
            var msgs = new SystemMessages();
            
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized");
                
                var location = Location.LoadByGooglePlaceId(json.googlePlaceId, msgs);

                if (location.IsEmpty())
                    location = new Location()
                    {
                        GooglePlaceId = json.googlePlaceId,
                        CreateDate = DateTime.UtcNow,
                        CreatedBy = currentUser?.ID,
                    };

                if (json.locationName.IsEmpty())
                    msgs.Add("Location Name is required", MessageType.UserError);

                if (json.city.IsEmpty())
                    msgs.Add("Location City is required", MessageType.UserError);

                if (json.state.IsEmpty())
                    msgs.Add("Location State is required", MessageType.UserError);

                if (json.country.IsEmpty())
                    msgs.Add("Location Country is required", MessageType.UserError);

                if (msgs.HasNoErrors())
                {
                    location.Name = json.locationName;
                    location.StreetNumber = json.streetNumber;
                    location.StreetName = json.streetName;
                    location.Address1 = $"{json.streetNumber} {json.streetName}";
                    location.Address2 = json.address2;
                    location.Neighborhood = json.neighborhood;
                    location.City = json.city;
                    location.County = json.county;
                    location.State = json.state;
                    location.PostalCode = json.postalCode;
                    location.Country = json.country;
                    location.Latitude = json.latitude;
                    location.Longitude = json.longitude;
                    location.LastModifiedDate = DateTime.UtcNow;
                    location.LastModifiedBy = currentUser?.ID;

                    //..co-ordiantes can be negative
                    if (json.longitude.IsNotEmpty() && json.latitude.IsNotEmpty())
                    {
                        var distance = DistanceAlgorithm.DistanceBetweenPlaces(Convert.ToDouble(json.longitude), Convert.ToDouble(json.latitude), 0, 0);
                        if (distance.IsNotEmpty())
                            location.DistanceFromOrigin = Convert.ToDecimal(distance);
                    }
                
                    msgs.AddRange(location.Save());
                }
                
                return Json(
                    new ApiResponse(msgs,location.ID),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs,null),
                    JsonRequestBehavior.AllowGet);
            }
        }


        public class ApiResponse
        {
            public SystemMessages msgs { get; set; }
            public dynamic obj { get; set; }

            public ApiResponse(SystemMessages msgs, dynamic obj)
            {
                this.msgs = msgs;
                this.obj = obj;
            }
        }

        public class WebUserDto
        {
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string userName { get; set; }

            public WebUserDto(WebUser currentUser)
            {
                this.firstName = currentUser.FirstName;
                this.lastName = currentUser.LastName;
                this.email = currentUser.Email;
                this.phone = currentUser.Phone;
                this.userName = currentUser.Username;
            }
            
        }
        
        public class FormDto
        {
            public List<FormQuestionDto> formQuestion { get; set; }
            public List<FormQuestionGroupDto> formQuestionGroup { get; set; }
            public List<FormQuestionAnswerDto> formQuestionAnswer { get; set; }

            public FormDto(List<FormQuestionDto> formQuestion, List<FormQuestionGroupDto> formQuestionGroup, List<FormQuestionAnswerDto> formQuestionAnswer)
            {
                this.formQuestion = formQuestion;
                this.formQuestionGroup = formQuestionGroup;
                this.formQuestionAnswer = formQuestionAnswer;
            }
            
        }

        public class FormQuestionDto
        {
            public int ID { get; set; }
            public int? FormQuestionGroupId { get; set; }
            public string Text { get; set; }
            public string Description { get; set; }
            public string Options { get; set; }
            public bool IsRequired { get; set; }
            public int? DisplayOrder { get; set; }
            public bool IsActive { get; set; }
            public bool IsDeleted { get; set; }
            public QuestionType QuestionTypeId { get; set; }
            public QuestionDataType QuestionDataTypeId { get; set; }
            public QuestionGroupDataType FormQuestionGroupQuestionGroupDataTypeId { get; set; }
        }
        
        public class FormQuestionAnswerDto
        {
            public int? FormQuestionId { get; set; }
            public int QuestionDataTypeId { get; set; }
            public string QuestionText { get; set; }
            public string Text { get; set; }
        }
        
        public class FormQuestionGroupDto
        {
            public int? FormId { get; set; }
            public string Name { get; set; }
            public int? DisplayOrder { get; set; }
            public QuestionGroupDataType QuestionGroupDataTypeId { get; set; }
        }

        public class QuestionAnswerResponse
        {
            public string groupid { get; set; }
            
            public string esignature { get; set; }
            public string answers { get; set; }
            
            public int? schoolLocationID { get; set; }
            
            public int? newChild { get; set; }
        }
        
        public class LocationDto
        {
            public string locationName { get; set; }
            public string streetNumber { get; set; }
            public string streetName { get; set; }
            public string address2 { get; set; }
            public string neighborhood { get; set; }
            public string city { get; set; }
            public string county { get; set; }
            public string state { get; set; }
            public string postalCode { get; set; }
            public string country { get; set; }
            public string googlePlaceId { get; set; }
            public decimal? latitude { get; set; }
            public decimal? longitude { get; set; }
        }
        
    }
}