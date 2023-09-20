using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using AfterSchool.Areas.Api.Models;
using AfterSchool.Helpers;
using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Areas.Api.Controllers
{
    public class RegistrationPinController : BaseController
    {
        [HttpGet]
        public ActionResult GetRegistrationSettings(string registrationUid)
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");
                
                var currentAccount = SessionVariables.CurrentAccount;
                if (currentAccount.IsEmpty())
                    throw new Exception("Unauthorized Account");

                if (!registrationUid.IsGuid())
                    throw new Exception("Invalid Id");

                var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);

                if (registration.IsEmpty())
                    throw new Exception("No Registration");

                var currentRegistrant = registration.GetRegistrant(msgs);

                if (currentRegistrant.IsEmpty())
                    throw new Exception("No Registrant");

                var currentMasterRegistrant = currentRegistrant.GetMasterRegistrant(msgs);
                if (currentMasterRegistrant.IsEmpty())
                    throw new Exception("No Master Registrant");

                var currentUserRegistrant = currentUser.GetMasterParentOrStudentOrAdultRegistrant(msgs);
                var participationObject = registration.GetFirstParticipationObject(msgs);

                if (participationObject.IsEmpty())
                    throw new Exception("Invalid Object");

                var answers = FormQuestionAnswer.LoadByRegistrantId(currentRegistrant.ID.Value, msgs);
                var additionalContactInfo = new List<PickUpDto>();

                var pickUpRegistrantWebUsers = RegistrantWebUser.LoadByRegistrantId(Convert.ToInt32(currentMasterRegistrant.ID), msgs);

                if(pickUpRegistrantWebUsers.Count == 1)
                {
                    additionalContactInfo.AddRange(GetAdditionalContactInfo(answers, QuestionDataType.Parent2FirstName,
                    QuestionDataType.Parent2LastName, QuestionDataType.Parent2MobilePhone, FormConstants.Parent2InfoToIncludeInStudentPanel()));

                    additionalContactInfo.AddRange(GetAdditionalContactInfo(answers, QuestionDataType.EmergencyContact1FirstName,
                        QuestionDataType.EmergencyContact1LastName, QuestionDataType.EmergencyContact1Phone,
                        FormConstants.EmergencyContact1InfoToIncludeInStudentPanel()));

                    additionalContactInfo.AddRange(GetAdditionalContactInfo(answers, QuestionDataType.EmergencyContact2FirstName,
                        QuestionDataType.EmergencyContact2LastName, QuestionDataType.EmergencyContact2Phone,
                        FormConstants.EmergencyContact2InfoToIncludeInStudentPanel()));

                    additionalContactInfo.AddRange(GetAdditionalContactInfo(answers, QuestionDataType.EmergencyContact3FirstName,
                        QuestionDataType.EmergencyContact3LastName, QuestionDataType.EmergencyContact3Phone,
                        FormConstants.EmergencyContact3InfoToIncludeInStudentPanel()));
                }

                
                foreach (var pickUpRegistrantWebUser in pickUpRegistrantWebUsers)
                {
                    var registeredPickUser = WebUser.Load(pickUpRegistrantWebUser.WebUserId.Value, msgs);
                    
                    if(pickUpRegistrantWebUser.AuthorizationTypeId == WebUserAuthorizationType.Pickup)
                        additionalContactInfo.Add(new PickUpDto()
                        {
                            ID = registeredPickUser.ID.Value,
                            FirstName = registeredPickUser.FirstName,
                            LastName = registeredPickUser.LastName,
                            Phone = registeredPickUser.Phone,
                            HasAccess = !pickUpRegistrantWebUser.IsDeleted,
                        });
                }

                dynamic response = new
                {
                    activityDto = new
                    {
                        Name = participationObject.Name,
                        Organization = participationObject.OwnerAccount.Name,
                        IsFree = participationObject.IsFree,
                        RegistrantType = currentUserRegistrant.PersonTypeId.ToString(),
                        FindAnotherActivityUrl = participationObject.OwnerAccount.ShortHomePageUrl,
                        ActivityUrl = participationObject.GetShortHomePageUrl(),
                        RegisterForNewChildUrl = participationObject.GetRegistrationPageUrl(),
                        IsGradeRestricted = participationObject.RestrictByGrade(),
                        SchoolLocation = currentRegistrant.GetSchoolLocation(msgs),
                        IsAgeRestricted = participationObject.AgeStart != null,
                        IsGenderRestricted = participationObject.IsGenderEnforced,
                        RegistrationIsCompleted = registration?.CompletionStatusId == RegistrationCompletionStatus.Completed,
                        CanAccess = currentUser.HasAccessToRegistrant(currentMasterRegistrant, msgs),
                        RegistrationUid = registration?.Uid,
                        IsAlreadyPaid = registration?.IsAlreadyPaidFor(msgs),
                        MinGrade = participationObject.GetMinGradeLevel(msgs).Abbreviation,
                        MaxGrade = participationObject.GetMaxGradeLevel(msgs).Abbreviation,
                        MinAge = participationObject.AgeStart,
                        MaxAge = participationObject.AgeEnd,
                        EnforcedGender = participationObject.GenderRestriction,
                        HeaderImageURL = AccountProfileHelper.GetHeaderImageURL(participationObject),
                        IsRequireCheckInCheckOut = participationObject.OwnerAccount.HasAccountPackageFeature(FeatureType.ManageCheckInCheckOut) && 
                                                   participationObject.IsRequireCheckInCheckOutEnabled.IsNotEmpty() &&
                                                   participationObject.IsRequireCheckInCheckOutEnabled,
                        StudentFirstName = currentRegistrant.FirstName,
                        StudentLastName = currentRegistrant.LastName,
                    },
                    setupDto = new
                    {
                        IsPhoneSetup = currentUser.Phone.IsNotEmpty(),
                        IsPinSetup = currentUser.PinCode.IsNotEmpty(),
                    },
                    additionalContactDto = additionalContactInfo,
                };

                return Json(
                    new ApiResponse(msgs, response),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddOrUpdateRegistrantWebUser(string registrationUid, string phone, string pinCode, List<PickUpDto> peopleList)
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");

                if (!registrationUid.IsGuid())
                    throw new Exception("Invalid Id");

                if (phone.IsNotEmpty() && WebUser.ExistsByPhone(phone.Trim(), msgs))
                    throw new Exception("Phone is already in use");

                var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);
                if (registration.IsEmpty())
                    throw new Exception("No Registration");

                var currentRegistrant = registration.GetRegistrant(msgs);
                if (currentRegistrant.IsEmpty())
                    throw new Exception("No Registrant");

                var currentMasterRegistrant = currentRegistrant.GetMasterRegistrant(msgs);
                if (currentMasterRegistrant.IsEmpty())
                    throw new Exception("No Master Registrant");
                
                if (currentUser.Phone.IsEmpty() || currentUser.PinCode.IsEmpty())
                {
                    if (currentUser.Phone.IsEmpty())
                        currentUser.Phone = phone;

                    if (currentUser.PinCode.IsEmpty())
                        currentUser.PinCode = pinCode;

                    msgs.AddRange(currentUser.Save());
                }
                
                var currentRegistrantWebUser = RegistrantWebUser.LoadByRegistrantIdByWebUserId(currentMasterRegistrant.ID.Value, 
                    currentUser.ID.Value, msgs);

                if (currentRegistrantWebUser.IsNotEmpty())
                {
                    currentRegistrantWebUser.AuthorizationTypeId = WebUserAuthorizationType.FullyAuthorized;
                    msgs.AddRange(currentRegistrantWebUser.Save());
                }

                if (peopleList.IsNotEmpty())
                    Database.ExecuteTransaction
                    (
                        (SystemMessages amsgs, DatabaseConnection adc) =>
                        {
                            peopleList.ForEach(currentPickUp =>
                            {
                                WebUser user = null;

                                if(currentPickUp.ID != -1)
                                {
                                    var existedRegistrantWebUser = RegistrantWebUser.LoadByRegistrantIdByWebUserId(currentMasterRegistrant.ID.Value,
                                        currentPickUp.ID, amsgs, adc);
                                    existedRegistrantWebUser.IsDeleted = !currentPickUp.HasAccess;

                                    amsgs.AddRange(existedRegistrantWebUser.Save(adc));

                                    return;
                                }


                                if (currentPickUp.Phone.IsNotEmpty() && !WebUser.ExistsByPhone(currentPickUp.Phone.Trim(), amsgs, adc))
                                    user = new WebUser
                                    {
                                        FirstName = currentPickUp.FirstName,
                                        LastName = currentPickUp.LastName,
                                        Phone = currentPickUp.Phone,
                                        CreateDate = DateTime.UtcNow,
                                        LastModifiedDate = DateTime.UtcNow,
                                    };
                                else if (currentPickUp.Phone.IsNotEmpty())
                                    user = WebUser.LoadByPhone(currentPickUp.Phone, amsgs, adc);
                                
                                if (user.IsNotEmpty())
                                {
                                    amsgs.AddRange(user.Save(adc));
                                    
                                    var registrantWebUser = new RegistrantWebUser
                                    {
                                        CreateDate = DateTime.UtcNow,
                                        RegistrantId = currentMasterRegistrant.ID,
                                        WebUserId = user.ID,
                                        IsDeleted = false,
                                        AuthorizationTypeId = WebUserAuthorizationType.Pickup,
                                    };
                                    amsgs.AddRange(registrantWebUser.Save(adc));

                                    if (TwilioHelper.IsValidPhoneNumber(currentPickUp.Phone))
                                        SendTwilioSms(registration.Uid.ToString(),currentPickUp.Phone);
                                }
                            });
                        },
                        (SystemMessages smsgs) => msgs.AddRange(smsgs),
                        (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                        (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                    );

                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult CreateNewPin(string registrationUid, string phone)
        {
            var msgs = new SystemMessages();
            try
            {
                if (!registrationUid.IsGuid())
                    throw new Exception("Invalid Id");

                var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);
                if (registration.IsEmpty())
                    throw new Exception("No Registration");

                if (phone.IsEmpty())
                    throw new Exception("Invalid Phone");

                var currentRegistrant = registration.GetRegistrant(msgs);
                if (currentRegistrant.IsEmpty())
                    throw new Exception("No Registrant");

                var currentMasterRegistrant = currentRegistrant.GetMasterRegistrant(msgs);
                if (currentMasterRegistrant.IsEmpty())
                    throw new Exception("No Master Registrant");

                var currentUser = WebUser.LoadByPhone(phone, msgs);
                if (currentUser.IsEmpty())
                    throw new Exception("Invalid User");

                if (!currentUser.HasAccessToRegistrant(currentMasterRegistrant, msgs))
                    throw new Exception("You are not authorized to access this page");
                
                var registrantWebUser = RegistrantWebUser.LoadByRegistrantIdByWebUserId(currentMasterRegistrant.ID.Value,
                    currentUser.ID.Value, msgs);
                
                if(registrantWebUser.IsEmpty())
                    throw new Exception("Invalid Registrant WebUser");
                
                if(registrantWebUser.IsDeleted)
                    throw new Exception("You are not authorized to access this page");

                var currentParentRegistrant = currentMasterRegistrant.GetParentOrGuardian(msgs);

                dynamic response = new
                {
                    currentUserInfo = new
                    {
                        IsPinSetup = currentUser.PinCode.IsNotEmpty(),
                        StudentFirstName = currentRegistrant.FirstName,
                        StudentLastName = currentRegistrant.LastName,
                        ParentFirstName = currentParentRegistrant?.FirstName,
                        ParentLastName = currentParentRegistrant?.LastName,
                    },
                };

                return Json(
                    new ApiResponse(msgs, response),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateNewPin(string registrationUid, string phone, string pinCode)
        {
            var msgs = new SystemMessages();
            try
            {
                if (!registrationUid.IsGuid())
                    throw new Exception("Invalid Id");

                var registration = Registration.LoadByUid(registrationUid.ToGuid(), msgs);
                if (registration.IsEmpty())
                    throw new Exception("No Registration");

                if (phone.IsEmpty())
                    throw new Exception("Invalid Phone");

                if (pinCode.IsEmpty())
                    throw new Exception("Invalid Pin");

                var currentRegistrant = registration.GetRegistrant(msgs);
                if (currentRegistrant.IsEmpty())
                    throw new Exception("No Registrant");

                var currentMasterRegistrant = currentRegistrant.GetMasterRegistrant(msgs);
                if (currentMasterRegistrant.IsEmpty())
                    throw new Exception("No Master Registrant");

                var currentUser = WebUser.LoadByPhone(phone, msgs);
                if (currentUser.IsEmpty())
                    throw new Exception("Invalid User");

                if (!currentUser.HasAccessToRegistrant(currentMasterRegistrant, msgs))
                    throw new Exception("You are not authorized to access this page");

                Database.ExecuteTransaction
                (
                    (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (currentUser.PinCode.IsEmpty())
                        {
                            currentUser.PinCode = pinCode;
                            amsgs.AddRange(currentUser.Save(adc));
                        }
                    },
                    (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );

                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetAllStudentsInfo(string accountName, string phone, string pinCode)
        {
            var msgs = new SystemMessages();
            try
            {
                if (accountName.IsEmpty())
                    throw new Exception("Invalid Account");

                if (phone.IsEmpty())
                    throw new Exception("Invalid Phone");

                if (pinCode.IsEmpty())
                    throw new Exception("Invalid Pin");

                var phoneCheck = phone;
                phoneCheck = phone.Contains("-") ? phoneCheck.Replace("-", "") : phoneCheck.Insert(3, "-").Insert(7, "-");
                var currentUser = WebUser.LoadByPhoneByPinCode(phone, pinCode, msgs);
                if (currentUser.IsEmpty())
                {
                    var currentUserNewPhone = WebUser.LoadByPhoneByPinCode(phoneCheck, pinCode, msgs);
                    if (currentUserNewPhone.IsEmpty())
                        throw new Exception("You are not authorized to access this page");
                    else
                        currentUser = currentUserNewPhone;
                }

                var account = Account.LoadByUniqueName(accountName, msgs);
                if (account.IsEmpty())
                    throw new Exception("Invalid Account");

                var masterRegistrants = currentUser.GetStudentRegistrants(msgs);
                var studentsInfoList = new List<CheckInCheckOutStudentInfoDto>();

                masterRegistrants.ForEach(masterRegistrant =>
                {
                    var registrants = Registrant.LoadByParentRegistrantIdByAccountId(masterRegistrant.ID.Value,
                        account.ID.Value, msgs);
                    var activityInfo = new List<ActivityDto>();

                    registrants.ForEach(registrant =>
                    {
                        var registrantActivity = RegistrantActivity.LoadByRegistrantId(registrant.ID.Value, msgs)
                            .FirstOrDefault(ra => ra.ActivityGroupId.IsEmpty());

                        if (registrantActivity.IsEmpty() || registrantActivity.AssignmentTypeId != RegistrationAssignmentType.Reserved)
                            return;

                        var activity = registrantActivity?.GetActivity(msgs);
                        var activitySchedule = activity?.GetSchedule(msgs);

                        var attendance = new Attendance();
                        var activityInstance = activity.HasSchedule ? activity.GetClosestActivityInstance(msgs) : null;

                        if (activity.HasSchedule)
                            attendance = Attendance.LoadByActivityInstanceId(activityInstance.ID.Value, msgs).FirstOrDefault();
                        else
                            attendance = Attendance.LoadByActivityIdByDate(activity.ID.Value, DateTime.UtcNow.Date, msgs).FirstOrDefault();
                        List<CheckInOut> checkInOutInfo = new List<CheckInOut>();
                        WebUser webUser = null;

                        if (attendance.IsNotEmpty())
                        {
                            var attendanceRegistrant = AttendanceRegistrant.LoadByAttendanceIdByRegistrantId(attendance.ID.Value,
                                registrant.ID.Value, msgs).FirstOrDefault();

                            if (attendanceRegistrant.IsNotEmpty())
                                checkInOutInfo = CheckInOut.LoadByAttendanceRegistrantId(attendanceRegistrant.ID.Value, msgs);
                        }

                        if (checkInOutInfo.Count > 0)
                            webUser = WebUser.Load(checkInOutInfo.Last().WebUserId, msgs);

                        var checkSetupInfo = new CheckInCheckOutSetupDto
                        {
                            CheckIn = checkInOutInfo.Count == 0 || checkInOutInfo.LastOrDefault().CheckType == CheckType.CheckOut ? false : true,
                            CheckInTime = checkInOutInfo.Count > 0 ? checkInOutInfo.Last().Date.ToShortTimeString() : null,
                            CheckInBy = webUser.IsNotEmpty() ? $"{webUser.FirstName} {webUser.LastName}" : "",
                        };

                        if (activity.IsNotEmpty() && account.HasAccountPackageFeature(FeatureType.ManageCheckInCheckOut) && 
                            activity.IsRequireCheckInCheckOut.IsNotEmpty() && activity.IsRequireCheckInCheckOut)
                            activityInfo.Add(new ActivityDto
                            {
                                StudentUid = registrant.Uid.ToString(),
                                ActivityUid = activity.Uid.ToString(),
                                ActivityName = activity.Name,
                                ActivityStartTime = activitySchedule?.DailyStartTime.ToShortTimeString(),
                                ActivityEndTime = activitySchedule?.DailyEndTime.ToShortTimeString(),
                                CheckSetupDto = checkSetupInfo,
                            });
                    });

                    if (activityInfo.Count > 0)
                        studentsInfoList.Add(new CheckInCheckOutStudentInfoDto
                        {
                            StudentName = masterRegistrant.Name,
                            Activities = activityInfo,
                        });
                });

                dynamic response = new
                {
                    AccountName = account.Name,
                    StudentsInfo = studentsInfoList,
                };

                return Json(
                    new ApiResponse(msgs, response),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddOrUpdateStudentCheckInOutInfo(string phone, string pinCode, string signature, List<CheckInOutStudentsDto> studentList)
        {
            var msgs = new SystemMessages();
            try
            {
                if (phone.IsEmpty())
                    throw new Exception("Invalid Phone");

                if (pinCode.IsEmpty())
                    throw new Exception("Invalid Pin");

                var currentUser = WebUser.LoadByPhoneByPinCode(phone, pinCode, msgs);
                if (currentUser.IsEmpty())
                    throw new Exception("You are not authorized to access this page");

                studentList.ForEach(student =>
                {
                    if (student.ActivityUid.IsEmpty() || !student.ActivityUid.IsGuid())
                        throw new Exception("Invalid Activity Id");
                    if (student.StudentUid.IsEmpty() || !student.StudentUid.IsGuid())
                        throw new Exception("Invalid Registrant Id");

                    var activity = Activity.LoadByUid(student.ActivityUid.ToGuid(), msgs);
                    if (activity.IsEmpty())
                        throw new Exception("No Activity");

                    var registrant = Registrant.LoadByUid(student.StudentUid.ToGuid(), msgs);
                    if (registrant.IsEmpty())
                        throw new Exception("No Registrant");


                    Database.ExecuteTransaction
                    (
                        (SystemMessages amsgs, DatabaseConnection adc) =>
                        {
                            var attendance = new Attendance();
                            var activityInstance = activity.HasSchedule ? activity.GetClosestActivityInstance(amsgs, adc) : null;

                            if (activity.HasSchedule)
                                attendance = Attendance.LoadByActivityInstanceId(activityInstance.ID.Value, amsgs, adc).FirstOrDefault();
                            else
                                attendance = Attendance.LoadByActivityIdByDate(activity.ID.Value, DateTime.UtcNow.Date, amsgs, adc).FirstOrDefault();

                            if (attendance.IsEmpty())
                            {
                                attendance = new Attendance
                                {
                                    CreateDate = DateTime.UtcNow,
                                    LastModifiedDate = DateTime.UtcNow,
                                    ActivityId = activity.ID,
                                    Date = DateTime.UtcNow.Date,
                                    IsCompleted = true,
                                    IsCharged = false,
                                    ActivityInstanceId = activityInstance?.ID
                                };
                                amsgs.AddRange(attendance.Save(adc));
                            }

                            var attendanceRegistrant = AttendanceRegistrant.LoadByAttendanceIdByRegistrantId(attendance.ID.Value,
                                registrant.ID.Value, amsgs, adc).FirstOrDefault();

                            if (attendanceRegistrant.IsEmpty())
                            {
                                attendanceRegistrant = new AttendanceRegistrant
                                {
                                    CreateDate = DateTime.UtcNow,
                                    LastModifiedDate = DateTime.UtcNow,
                                    RegistrantId = registrant.ID,
                                    AttendanceId = attendance.ID,
                                    IsPresent = false,
                                    IsLate = false,
                                    NumberPresent = 1,
                                    NumberLate = 0,
                                    NumberPossible = 1,
                                };
                                amsgs.AddRange(attendanceRegistrant.Save(adc));
                            }

                            if (signature.IsEmpty())
                                if (Request.Files[0].ContentLength > 0)
                                {
                                    var file = Request.Files[0].InputStream;
                                    var fileName = Request.Files[0].FileName;
                                    DateTime currentUtcDateTime = DateTime.UtcNow;
                                    string uniqueDate = currentUtcDateTime.ToString("yyyyMMdd_HHmmss");
                                    string fileExtension = Path.GetExtension(fileName);
                                    string newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{uniqueDate}{fileExtension}";
                                    signature = GetUploadedFilePath(registrant, file, newFileName, amsgs, adc);
                                }

                            var checkInOutInfo = new CheckInOut
                            {
                                CreateDate = DateTime.UtcNow,
                                LastModifiedDate = DateTime.UtcNow,
                                Date = DateTime.UtcNow,
                                Signature = signature,
                                Browser = Request.UserAgent,
                                IPAddress = Request.GetUserHostAddress(),
                                WebUserId = currentUser.ID.Value,
                                AttendanceRegistrantId = attendanceRegistrant.ID.Value,
                                CheckType = student.CheckInOut ? CheckType.CheckIn : CheckType.CheckOut,
                            };
                            amsgs.AddRange(checkInOutInfo.Save(adc));
                        },
                        (SystemMessages smsgs) => msgs.AddRange(smsgs),
                        (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                        (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                    );
                });

                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SendResetUrl(string phone, string account)
        {
            var msgs = new SystemMessages();
            var token = new PasswordResetToken();
            try
            {
                if (phone.IsEmpty())
                    throw new Exception("Invalid Phone");

                var webUser = WebUser.LoadByPhone(phone, msgs);

                if(webUser.IsEmpty())
                    throw new Exception("Phone not registered");

                if (!TwilioHelper.IsValidPhoneNumber(phone))
                    throw new Exception("Invalid Phone Number");

                if (!PasswordResetToken.ExistsAndIsValidByWebUserId(webUser.ID.Value))
                {
                    token = new PasswordResetToken
                    {
                        ExpireDate = DateTime.UtcNow.AddMinutes(15),
                        CreateDate = DateTime.UtcNow,
                        CreatedBy = webUser.ID,
                        WebUserId = webUser.ID,
                    };
                }
                else
                {
                    token = PasswordResetToken.LoadValidByWebUserId(webUser.ID.Value);
                    token.ExpireDate = DateTime.UtcNow.AddMinutes(15);
                }

                token.LastModifiedDate = DateTime.UtcNow;
                token.LastModifiedBy = webUser.ID.Value;
                msgs.AddRange(token.Save());

                string msgBody = $"Click the link below to reset your AfterSchool HQ PIN code. " +
                    $"If you need more help or did not request a PIN reset, feel free to contact us at support@afterschoolhq.com." +
                    $"\r\nReset PIN: {Utilities.ShortBaseUrl()}/reactapp/check-in-out/reset-pin?token={token.Uid}";

                if (account.IsNotEmpty())
                    msgBody = msgBody + $"&accountName={account}";

                TwilioHelper.SendSMS(
                    webUser.Phone,
                    msgBody,
                    null,
                    null,
                    msgs);

                return Json(
                new ApiResponse(msgs,
                    null),
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ResetPin(string token, string code)
        {
            var msgs = new SystemMessages();
            try
            {
                if (!PasswordResetToken.ExistsAndIsValidByUid(token.ToGuid()))
                    throw new Exception("Invalid Token");

                if (code.IsEmpty())
                    throw new Exception("Code is not valid");

                var passwordResetToken = PasswordResetToken.LoadValidByUid(token.ToGuid());
                var webUser = WebUser.Load(passwordResetToken.WebUserId.Value, msgs);

                webUser.PinCode = code;
                passwordResetToken.ExpireDate = DateTime.UtcNow;
                
                msgs.AddRange(webUser.Save());
                msgs.AddRange(passwordResetToken.Save());
                
                return Json(
                new ApiResponse(msgs,
                    null),
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetAuthorizedPersonsList()
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");
                
                var currentUserRegistrant = currentUser.GetMasterParentOrStudentOrAdultRegistrant(msgs);

                var masterRegistrants = currentUser.GetStudentRegistrants(msgs);
                var studentsList = GetAuthorizedPerson(masterRegistrants, msgs);

                dynamic response = new
                {
                    Students = studentsList,
                    IsPinSetup = currentUser.PinCode.IsNotEmpty(),
                    IsPhoneSetup = currentUser.Phone.IsNotEmpty(),
                    RegistrantType = currentUserRegistrant?.PersonTypeId.ToString(),
                };

                return Json(
                    new ApiResponse(msgs, response),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AddOrUpdateAuthorizedPersons(List<ManageCheckInOutAccessDto> studentsList)
        {
            var msgs = new SystemMessages();

            var currentUser = SessionVariables.CurrentUser;
            if (currentUser.IsEmpty())
                throw new Exception("Unauthorized User");
            
            var currentUserRegistrant = currentUser.GetMasterParentOrStudentOrAdultRegistrant(msgs);

            try
            {
                studentsList.ForEach(student =>
                {
                    if (student.StudentUid.IsEmpty() || !student.StudentUid.IsGuid())
                        throw new Exception("Invalid Registrant Id");

                    if (student.StudentName.IsEmpty())
                        throw new Exception("Invalid Registrant Name");

                    var masterRegistrant = Registrant.LoadByUid(student.StudentUid.ToGuid(), msgs);

                    Database.ExecuteTransaction
                    (
                        (SystemMessages amsgs, DatabaseConnection adc) =>
                        {
                            student.AuthorizedPersons.ForEach(authorizedPerson =>
                            {
                                WebUser user;
                                RegistrantWebUser registrantWebUser;

                                if (authorizedPerson.ID == -1 && amsgs.HasNoErrors())
                                {
                                    if (WebUser.ExistsByPhone(authorizedPerson.Phone, amsgs))
                                        user = WebUser.LoadByPhone(authorizedPerson.Phone, amsgs, adc);
                                    else
                                    {  
                                        user = new WebUser
                                        {
                                        FirstName = authorizedPerson.FirstName,
                                        LastName = authorizedPerson.LastName,
                                        Phone = authorizedPerson.Phone,
                                        CreateDate = DateTime.UtcNow,
                                        LastModifiedDate = DateTime.UtcNow,
                                        };
                                    amsgs.AddRange(user.Save(adc));
                                    }
                                    var existingRegistrantWebUser = RegistrantWebUser.LoadByRegistrantIdByWebUserId(masterRegistrant.ID.Value,
                                        user.ID.Value, amsgs, adc);

                                    if(existingRegistrantWebUser.IsEmpty())
                                    {
                                        registrantWebUser = new RegistrantWebUser
                                        {
                                            CreateDate = DateTime.UtcNow,
                                            RegistrantId = masterRegistrant.ID,
                                            WebUserId = user.ID,
                                            IsDeleted = false,
                                            AuthorizationTypeId = WebUserAuthorizationType.Pickup,
                                        };
                                        amsgs.AddRange(registrantWebUser.Save(adc));
                                    }

                                    if (amsgs.HasNoErrors())
                                    {
                                        var registrant = Registrant.LoadByParentRegistrantId(masterRegistrant.ID.Value, amsgs, adc).LastOrDefault();
                                        var registration = Registration.LoadByRegistrantId(registrant.ID.Value, amsgs, adc).LastOrDefault();

                                        if (registration.IsNotEmpty() && TwilioHelper.IsValidPhoneNumber(authorizedPerson.Phone))
                                            SendTwilioSms(registration.Uid.ToString(), authorizedPerson.Phone);
                                    }
                                    
                                }
                                else
                                {
                                    user = WebUser.Load(authorizedPerson.ID, amsgs, adc);

                                    if(!user.Phone.Equals(authorizedPerson.Phone))
                                        if(!WebUser.ExistsByPhone(authorizedPerson.Phone, amsgs))
                                            user.Phone = authorizedPerson.Phone;
                                        else
                                        {
                                            amsgs.Add($"Phone already exist {authorizedPerson.Phone}", MessageType.UserError);
                                            return;
                                        }

                                    if (user.IsNotEmpty() && amsgs.HasNoErrors())
                                    {
                                        user.FirstName = authorizedPerson.FirstName;
                                        user.LastName = authorizedPerson.LastName;

                                        amsgs.AddRange(user.Save(adc));

                                        registrantWebUser = RegistrantWebUser.LoadByRegistrantIdByWebUserId(masterRegistrant.ID.Value,
                                                user.ID.Value, amsgs, adc);

                                        registrantWebUser.IsDeleted = !authorizedPerson.HasAccess;

                                        if (registrantWebUser.IsDeleted)
                                        {
                                            EmailHelper.SendCheckInCheckOutRevokeAccessEmail(user.DisplayName, masterRegistrant.FirstName, currentUserRegistrant.Email, currentUserRegistrant.FirstName);

                                            var smsBody = $"This is an automated message from AfterSchool HQ. " +
                                            $"You are no longer an authorized person for {masterRegistrant.DisplayName}. " +
                                            $"If you believe this is an error,  please contact {masterRegistrant.FirstName}’s parent or guardian.";

                                            TwilioHelper.SendSMS(
                                                user.Phone,
                                                smsBody,
                                                null,
                                                null,
                                                msgs);
                                        }

                                        amsgs.AddRange(registrantWebUser.Save(adc));
                                    }
                                }
                            });
                        },
                        (SystemMessages smsgs) => msgs.AddRange(smsgs),
                        (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                        (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                    );
                });

                var masterRegistrants = currentUser.GetStudentRegistrants(msgs);

                dynamic response = new
                {
                    Students = GetAuthorizedPerson(masterRegistrants, msgs),
                    IsPinSetup = currentUser.PinCode.IsNotEmpty(),
                    IsPhoneSetup = currentUser.Phone.IsNotEmpty(),
                    RegistrantType = currentUserRegistrant?.PersonTypeId.ToString(),
                };

                return Json(
                    new ApiResponse(msgs, response),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ResendInvite(string studentUid, int webUserId)
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;

                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");
                
                if (studentUid.IsEmpty() || !studentUid.IsGuid())
                    throw new Exception("Invalid Registrant Id");

                if(webUserId.IsEmpty())
                    throw new Exception("Invalid WebUserId Id");

                var masterRegistrant = Registrant.LoadByUid(studentUid.ToGuid(), msgs);

                if (!currentUser.HasAccessToRegistrant(masterRegistrant, msgs))
                    throw new Exception("Do not hav access");

                var user = WebUser.Load(webUserId, msgs);

                var registrant = Registrant.LoadByParentRegistrantId(masterRegistrant.ID.Value, msgs).LastOrDefault();

                var registration = Registration.LoadByRegistrantId(registrant.ID.Value, msgs).LastOrDefault();

                if (registration.IsNotEmpty() && TwilioHelper.IsValidPhoneNumber(user.Phone))
                    SendTwilioSms(registration.Uid.ToString(), user.Phone);

                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CreateNewPinForParent(string pinCode, string phone)
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;

                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");

                if (phone.IsNotEmpty() && WebUser.ExistsByPhone(phone.Trim(), msgs))
                    throw new Exception("Phone is already in use");
                
                Database.ExecuteTransaction
                (
                    (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (currentUser.Phone.IsEmpty() || currentUser.PinCode.IsEmpty())
                        {
                            if (currentUser.Phone.IsEmpty())
                                currentUser.Phone = phone;

                            if (currentUser.PinCode.IsEmpty())
                                currentUser.PinCode = pinCode;

                            currentUser.LastModifiedDate = DateTime.UtcNow;
                            amsgs.AddRange(currentUser.Save(adc));

                            var registrantWebUsers = RegistrantWebUser.LoadByWebUserIdByIsDeleted(currentUser.ID.Value, false, amsgs, adc);
                            registrantWebUsers.ForEach(registrantWebUser =>
                            {
                                registrantWebUser.AuthorizationTypeId = WebUserAuthorizationType.FullyAuthorized;
                                amsgs.AddRange(registrantWebUser.Save(adc));
                            });
                        }
                    },
                    (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );

                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateExistingPinForParent(string oldPinCode, string newPinCode)
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;

                if (currentUser.IsEmpty())
                    throw new Exception("Unauthorized User");

                if (currentUser.PinCode.IsNotEmpty() && !currentUser.PinCode.Equals(oldPinCode))
                    throw new Exception("Pin is incorrect");
                
                currentUser.UpdatePinCode(newPinCode, msgs);
                
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        private static List<PickUpDto> GetAdditionalContactInfo(List<FormQuestionAnswer> answers, QuestionDataType firstName, QuestionDataType lastName,
            QuestionDataType phone, List<QuestionDataType> questionDataTypes)
        {
            var pickUpContactList = new List<PickUpDto>();
            var contactInfo = answers.Where(x => x.QuestionDataTypeId.In(questionDataTypes) && x.Text.IsNotEmpty() && 
                                                 !x.Text.Contains("null")).ToList();

            if (contactInfo.Count > 0)
                pickUpContactList.Add(new PickUpDto()
                {
                    ID = -1,
                    FirstName = contactInfo.FirstOrDefault(x => x.QuestionDataTypeId == firstName)?.Text,
                    LastName = contactInfo.FirstOrDefault(x => x.QuestionDataTypeId == lastName)?.Text,
                    Phone = contactInfo.FirstOrDefault(x => x.QuestionDataTypeId == phone)?.Text,
                    PersonPhoneDataType = phone,
                    HasAccess = false,
                });
            return pickUpContactList;
        }

        private static string GetUploadedFilePath(Registrant registrant, Stream file, string filename, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            var ah = Utilities.GetNewAmazonHelper();

            var regEx = new Regex(
                BasicLibrary.Utilities.UrlFriendlyRegEx,
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            );

            var account = registrant.GetAccount(msgs, dc);
            var accountFolder = $"{account.UniqueName}-{account.ID}";
            var registrantFolder = $"{registrant.Name.Replace(regEx, "")}-{registrant.ID}";
            var amazonFilePath = $"{accountFolder}/{registrantFolder}/{filename}".ToLower();

            msgs.AddRange(ah.AddFile(file, amazonFilePath));

            return ah.GetFileLink(amazonFilePath);
        }

        private static void SendTwilioSms(string regUid, string phone)
        {
            var msgs = new SystemMessages();
            var url = $"{Utilities.BaseUrl()}/reactapp/pin-setup/{regUid}?phone={phone}";
            var msgBody = $"Welcome to {Utilities.AppName()}! You've been invited for an activity that will require you to check in " +
                          "and check out your child each day. \nPlease setup your PIN Number that you will use to check in and " +
                          $"check out your child through this link: {url}";

            TwilioHelper.SendSMS(
                phone,
                msgBody,
                null,
                null,
                msgs);
        }

        public static List<ManageCheckInOutAccessDto> GetAuthorizedPerson(List<Registrant> masterRegistrants , SystemMessages msgs)
        {
            var studentsList = new List<ManageCheckInOutAccessDto>();

            masterRegistrants.ForEach(masterRegistrant =>
            {
                var registrantWebUsers = RegistrantWebUser.LoadByRegistrantId(masterRegistrant.ID.Value, msgs);
                var authorizedPersons = new List<PickUpDto>();

                registrantWebUsers.ForEach(registrantWebUser =>
                {
                    var authorizedPerson = WebUser.Load(registrantWebUser.WebUserId.Value);

                    if (registrantWebUser.AuthorizationTypeId == WebUserAuthorizationType.Pickup)
                        authorizedPersons.Add(new PickUpDto
                        {
                            ID = authorizedPerson.ID.Value,
                            FirstName = authorizedPerson.FirstName,
                            LastName = authorizedPerson.LastName,
                            HasAccess = !registrantWebUser.IsDeleted,
                            Phone = authorizedPerson.Phone,
                            IsPinSetup = authorizedPerson.PinCode.IsNotEmpty(),
                        });
                });

                studentsList.Add(new ManageCheckInOutAccessDto
                {
                    StudentName = masterRegistrant.Name,
                    StudentUid = masterRegistrant.Uid.ToString(),
                    AuthorizedPersons = authorizedPersons,
                });
            });

            return studentsList;
        }
    }
}