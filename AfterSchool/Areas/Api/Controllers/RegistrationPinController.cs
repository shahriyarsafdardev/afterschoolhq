using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using AfterSchool.Areas.Api.Models;
using AfterSchool.Helpers;
using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Microsoft.IdentityModel.Tokens;
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

                var pickUpRegistrantWebUsers = RegistrantWebUser.LoadByRegistrantIdBy(Convert.ToInt32(currentMasterRegistrant.ID), msgs);

                if(pickUpRegistrantWebUsers.Count() == 1)
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
                    
                    if(registeredPickUser.AuthorizationTypeId == WebUserAuthorizationType.Pickup)
                        additionalContactInfo.Add(new PickUpDto()
                        {
                            ID = registeredPickUser.ID.Value,
                            FirstName = registeredPickUser.FirstName,
                            LastName = registeredPickUser.LastName,
                            Phone = registeredPickUser.Phone,
                            HasAccess = !pickUpRegistrantWebUser.IsDeleted
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
                        UserAuthorizationType = currentUser.AuthorizationTypeId.ToString(),
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

                    currentUser.AuthorizationTypeId = WebUserAuthorizationType.FullyAuthorized;
                    msgs.AddRange(currentUser.Save());
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
                                    var existedRegistrantWebUser = RegistrantWebUser.LoadByRegistrantIdByWebUserId(currentMasterRegistrant.ID.Value, currentPickUp.ID, amsgs, adc);
                                    existedRegistrantWebUser.IsDeleted = !currentPickUp.HasAccess;

                                    amsgs.AddRange(existedRegistrantWebUser.Save(adc));

                                    return;
                                }


                                if (currentPickUp.Phone.IsNotEmpty() && !WebUser.ExistsByPhone(currentPickUp.Phone.Trim(), amsgs, adc))
                                {
                                    var url = $"{Utilities.BaseUrl()}/reactapp/pin-setup/{registration.Uid}?phone={currentPickUp.Phone}";
                                    var msgBody = $"Welcome to {Utilities.AppName()}! You've been invited for an activity that will require you to check in " +
                                                  "and check out your child each day. \nPlease setup your PIN Number that you will use to check in and " +
                                                  $"check out your child through this link: {url}";

                                    user = new WebUser
                                    {
                                        FirstName = currentPickUp.FirstName,
                                        LastName = currentPickUp.LastName,
                                        Phone = currentPickUp.Phone,
                                        CreateDate = DateTime.UtcNow,
                                        LastModifiedDate = DateTime.UtcNow,
                                        AuthorizationTypeId = WebUserAuthorizationType.Pickup,
                                    };
                                    amsgs.AddRange(user.Save(adc));
                                }
                                else if (currentPickUp.Phone.IsNotEmpty())
                                {
                                    user = WebUser.LoadByPhone(currentPickUp.Phone, amsgs, adc);
                                }
                                if (user.IsNotEmpty())
                                {
                                    var registrantWebUser = new RegistrantWebUser
                                    {
                                        CreateDate = DateTime.UtcNow,
                                        RegistrantId = currentMasterRegistrant.ID,
                                        WebUserId = user.ID,
                                        IsDeleted = false,
                                    };
                                    amsgs.AddRange(registrantWebUser.Save(adc));

                                    //     TwilioHelper.SendSMS(
                                    //         currentPickUp.Phone,
                                    //         msgBody,
                                    //         null,
                                    //         null,
                                    //         amsgs);
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
                        var activity = registrantActivity?.GetActivity(msgs);
                        var activitySchedule = activity?.GetSchedule(msgs);
                        var attendance = Attendance.LoadByActivityIdByDate(activity.ID.Value, DateTime.UtcNow.Date, msgs).FirstOrDefault();
                        List<CheckInOut> checkInOutInfo = null;
                        WebUser webUser = null;

                        if (attendance.IsNotEmpty())
                        {
                            var attendanceRegistrant = AttendanceRegistrant.LoadByAttendanceIdByRegistrantId(attendance.ID.Value,
                                registrant.ID.Value, msgs).FirstOrDefault();

                            if (attendanceRegistrant.IsNotEmpty())
                                checkInOutInfo = CheckInOut.LoadByAttendanceRegistrantId(attendanceRegistrant.ID.Value, msgs);
                        }

                        if (checkInOutInfo.IsNotEmpty())
                            webUser = WebUser.Load(checkInOutInfo.First().WebUserId, msgs);

                        var checkSetupInfo = new CheckInCheckOutSetupDto
                        {
                            CheckIn = checkInOutInfo.IsEmpty() || checkInOutInfo.LastOrDefault().CheckType == CheckType.CheckOut ? false : true,
                            CheckInTime = checkInOutInfo?.First().Date.ToShortTimeString(),
                            CheckInBy = webUser.IsNotEmpty() ? $"{webUser.FirstName} {webUser.LastName}" : "",
                        };

                        if (activity.IsNotEmpty() && activity.IsRequireCheckInCheckOut.IsNotEmpty() && activity.IsRequireCheckInCheckOut)
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
                            var attendance = Attendance.LoadByActivityIdByDate(activity.ID.Value, DateTime.UtcNow.Date, amsgs, adc).FirstOrDefault();
                            if (attendance.IsEmpty())
                            {
                                attendance = new Attendance
                                {
                                    CreateDate = DateTime.UtcNow,
                                    LastModifiedDate = DateTime.UtcNow,
                                    ActivityId = activity.ID,
                                    Date = DateTime.UtcNow.Date,
                                    IsCompleted = false,
                                    IsCharged = false,
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
                                    NumberPresent = 0,
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
            try
            {
                if (phone.IsEmpty())
                    throw new Exception("Invalid Phone");

                var webUser = WebUser.LoadByPhone(phone, msgs);

                if(webUser.IsEmpty())
                    throw new Exception("Phone not registered");

                var token = new PasswordResetToken
                {
                    ExpireDate = DateTime.UtcNow.AddMinutes(10),
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = webUser.ID,
                    WebUserId = webUser.ID
                };

                token.LastModifiedDate = DateTime.UtcNow;
                token.LastModifiedBy = webUser.ID.Value;
                msgs.AddRange(token.Save());

                string msgBody = $"Click the link below to reset your AfterSchool HQ PIN code. " +
                    $"If you need more help or did not request a PIN reset, feel free to contact us at support@afterschoolhq.com." +
                    $"\r\nReset PIN: {Utilities.ShortBaseUrl()}/reactapp/check-in-out/reset-pin/?token={token.Uid}";

                if (account.IsNotEmpty())
                    msgBody = msgBody + $"&account={account}";

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

        private static List<PickUpDto> GetAdditionalContactInfo(List<FormQuestionAnswer> answers, QuestionDataType firstName, QuestionDataType lastName,
            QuestionDataType phone, List<QuestionDataType> questionDataTypes)
        {
            var pickUpContactList = new List<PickUpDto>();
            var contactInfo = answers.Where(x => x.QuestionDataTypeId.In(questionDataTypes) && x.Text.IsNotEmpty()).ToList();

            if (contactInfo.Count > 0)
                pickUpContactList.Add(new PickUpDto()
                {
                    ID = -1,
                    FirstName = contactInfo.FirstOrDefault(x => x.QuestionDataTypeId == firstName)?.Text,
                    LastName = contactInfo.FirstOrDefault(x => x.QuestionDataTypeId == lastName)?.Text,
                    Phone = contactInfo.FirstOrDefault(x => x.QuestionDataTypeId == phone)?.Text,
                    PersonPhoneDataType = phone,
                    HasAccess = false
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

            return amazonFilePath;
        }
    }
}