using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using Email = AfterSchoolLibrary.Helpers.Email;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary.Services
{
    public static class RegistrationService
    {
        public static void SendLotteryConfirmation(this Registration registration, Account billingAccount,
            ActivityGroup group, WebUser loggedInUser, bool sendBcc, SystemMessages msgs, DatabaseConnection dc = null)
        {
            EmailHelper.SendLotteryResults(registration, billingAccount, group.GetParticipationObject(msgs, dc),
                loggedInUser, sendBcc, msgs, dc);
        }

        public static void SendConfirmation(this Registration registration, CheckoutObject checkoutObject,
            string customMessage, WebUser loggedInUser, bool sendBcc, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (checkoutObject.ParticipationObject.ActivityType == ActivityType.VolunteerOpportunity)
            {
                msgs.AddRange(EmailHelper.SendVolunteerRegistrationConfirmed(checkoutObject, customMessage, loggedInUser, sendBcc, dc));
                return;
            }

            msgs.AddRange(EmailHelper.SendRegistrationConfirmed(checkoutObject, customMessage, loggedInUser, false, sendBcc, dc));

            // if the person registering is the student
            // TODO: Handle better if provider on the backend is registering the student
            //var masterRegistrant = loggedInUser.GetMasterParentOrStudentRegistrant(msgs, dc);
            var masterRegistrant = loggedInUser.GetMasterParentOrStudentOrAdultRegistrant(msgs, dc);

            // this is the email that is sent to parents if the main registrant is the student
            // ** PARENT EMAIL if student is registering
            if (masterRegistrant.IsNotEmpty() && masterRegistrant.IsStudent)
                msgs.AddRange(EmailHelper.SendRegistrationConfirmed(checkoutObject, customMessage, loggedInUser, true,
                    false, dc));
        }

        public static void SendInterestConfirmation(this Registration registration, CheckoutObject checkoutObject,
            string customMessage, WebUser loggedInUser, bool sendBcc, SystemMessages msgs, DatabaseConnection dc = null)
        {
            EmailHelper.SendInterestConfirmation(checkoutObject, customMessage, loggedInUser, msgs, dc);
        }

        public static void DeleteWithRelatedRecords(this Registration registration, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // right now there is one new registrant record per registration
                    // so we can know that it's okay to delete the registrant
                    // TODO: Notice that this may be a dangerous assumption one day
                    // Registration activities are deleted with the registrant
                    registration.GetRegistrant(amsgs, adc).DeleteWithRelatedRecords(amsgs, adc);
                    registration.DeleteSessionActions(amsgs, adc);
                    registration.DeleteAnswers(amsgs, adc);
                    amsgs.AddRange(registration.Delete(adc));
                    registration.UpdateRelatedRegistrationsAfterDelete(amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        public static void MergeInto(this Registration registration, Registration mergedFrom, int loggedInUserId,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    if (registration.RegistrationPeriodId != mergedFrom.RegistrationPeriodId)
                    {
                        amsgs.Add("You can only merge registrations from the same registration window",
                            MessageType.UserError);
                    }

                    if (amsgs.HasNoErrors())
                    {
                        var mergedFromActivities = mergedFrom.GetRegistrantActivities(amsgs, adc);

                        mergedFromActivities.ForEach(activity =>
                            registration.MoveActivity(activity, loggedInUserId, amsgs, adc));

                        // delete previous registration
                        if (amsgs.HasNoErrors())
                            mergedFrom.DeleteWithRelatedRecords(amsgs, adc);

                        registration.LastModifiedBy = loggedInUserId;
                        registration.LastModifiedDate = DateTime.UtcNow;
                        amsgs.AddRange(registration.Save(adc));
                    }
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        public static void CancelAll(this Registration registration, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            registration.DeleteWithRelatedRecords(msgs, dc);
        }

        internal static RegistrationAssignmentType GetRegistrationAssignmentType(ParticipationObject participationObject, Registrant masterRegistrant,
            int numberOfSeatsRequested, RegistrationAssignmentType? existingAssignmentTypeId, decimal? amountPaidByRegistrant, int? loggedInWebUserId,
            bool requiresBackgroundCheck)
        {
            var seatsAvailable = participationObject.SeatsRemaining;

            if (participationObject.RegistrationMode == RegistrationMode.Apply)
                return RegistrationAssignmentType.Applied;

            // if they're paying for a registration they already completed
            // or could be just e-signing for a student so this will need to be double checked
            if (existingAssignmentTypeId.HasValue && existingAssignmentTypeId.Value.In(Registration.ReservedStatuses))
                return RegistrationAssignmentType.Reserved;

            // if there is room
            if (!seatsAvailable.HasValue || seatsAvailable >= numberOfSeatsRequested)
            {
                var isStudentOrTeacherRegistering = (masterRegistrant.IsEmpty() ||
                                                    (masterRegistrant.WebUserId.HasValue && loggedInWebUserId == masterRegistrant.WebUserId)) &&
                                                    masterRegistrant.PersonTypeId != RegistrantPersonType.Adult;

                // if the person registering is the student
                if (participationObject.RequiresParentalConsent && isStudentOrTeacherRegistering)
                    return RegistrationAssignmentType.ReservedPendingParentalConsent;

                var registrantHasNotYetPaid = !amountPaidByRegistrant.HasValue || amountPaidByRegistrant.Value == 0m;

                if (!participationObject.IsFree && registrantHasNotYetPaid && !participationObject.PostPayPlans.Any())
                    return RegistrationAssignmentType.ReservedPendingPayment;

                if (requiresBackgroundCheck)
                    return RegistrationAssignmentType.ReservedPendingBackgroundCheck;

                return RegistrationAssignmentType.Reserved;
            }

            // this is probably temporary and will break if something else changes
            // assuming that for sessions there are no reserved seat setting saved
            if (participationObject.IsActivityGroup() &&
                participationObject.GetActivityGroup().ActivityGroupTypeId == ActivityGroupType.Session)
                return RegistrationAssignmentType.Reserved;

            // forget the limit on waiting list seats, just dump people there for now
            // TODO: figure out what to do if paying and only waiting list seats left
            return RegistrationAssignmentType.WaitingList;
        }

        public static void ProcessRegistrationAssignment(CheckoutObject checkoutObject, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            ProcessRegistrationAssignment(
                checkoutObject.Registration,
                checkoutObject.ParticipationObject,
                checkoutObject.MasterRegistrant,
                checkoutObject.Registration.GetRegistrant(msgs, dc),
                checkoutObject.NumberOfSeats,
                1,
                checkoutObject.ParticipationObject.IsActivity()
                    ? checkoutObject.ParticipationObject.GetAnyRelatedActivityGroupId()
                    : new int?(),
                checkoutObject.ParticipationObject.GetCurrentRegistrationPeriod(msgs, dc)?.ID,
                checkoutObject.GetTotalCostNoFees(msgs, dc),
                checkoutObject.SelectedDates,
                loggedInUser,
                msgs,
                dc);
        }

        public static void ProcessRegistrationAssignment(this Registration registration,
            ParticipationObject participationObject, Registrant masterRegistrant, Registrant registrant,
            int seatsRequested, int? activityGroupId, int? periodId, decimal? amountCharged, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            registration.ProcessRegistrationAssignment(
                participationObject,
                masterRegistrant,
                registrant,
                seatsRequested,
                1,
                activityGroupId,
                periodId,
                amountCharged,
                null,
                loggedInUser,
                msgs,
                dc);
        }

        public static void ProcessRegistrationAssignment(this Registration registration,
            ParticipationObject participationObject, Registrant masterRegistrant, Registrant registrant,
            int seatsRequested, int preferenceNumber, int? activityGroupId, int? periodId, decimal? amountCharged,
            List<DateTime> registeredDates, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            // NOTE: getting only one activity per registration at the moment, 
            // but could be problem in the future
            var ra = registration.GetRegistrantActivity(participationObject, msgs, dc);

            var amountPaidByRegistrant = ra.IsNotEmpty() && ra.RegistrantId.HasValue
                ? participationObject.GetAmountPaidByRegistrant(ra.RegistrantId.Value, msgs, dc)
                : (decimal?) null;
            var requiresBackgroundCheck = IsBackgroundCheckRequired(participationObject.ActivityType, loggedInUser.ID);
            var assignmentType = GetRegistrationAssignmentType(participationObject, masterRegistrant, seatsRequested, ra?.AssignmentTypeId,
                amountPaidByRegistrant, loggedInUser.ID, requiresBackgroundCheck);

            if (assignmentType == RegistrationAssignmentType.NotSet)
            {
                msgs.Add(
                    $"Sorry, there are no more reserved seats or waiting list seats available for {participationObject.Name}",
                    MessageType.UserError);
                return;
            }

            // add or update the registrant for the activity
            ra = registrant.AddOrUpdateParticipationObject(
                registrationId: registration.ID.Value,
                participationObjectId: participationObject.ObjectID.Value,
                participationObjectType: participationObject.ObjectType,
                assignmentType: assignmentType,
                preferenceNumber: preferenceNumber,
                registrationPeriodId: periodId,
                activityGroupId: activityGroupId,
                amountCharged: amountCharged,
                loggedInUserId: loggedInUser.ID.Value,
                msgs: msgs,
                dc: dc);

            if (registeredDates.IsNotEmpty() && registeredDates.Any())
            {
                registeredDates.ForEach(date =>
                {
                    var instance = participationObject.Schedule.GetInSessionInstanceByDate(date, msgs, dc);

                    if (instance.IsEmpty())
                        instance = participationObject.Schedule.AddInstance(date, loggedInUser, msgs, dc);

                    var instanceSeats = instance.GetSeatsByRegistrantActivity(ra, msgs, dc);

                    if (instanceSeats.IsEmpty())
                        instanceSeats = instance.AddInstanceSeats(ra, loggedInUser, msgs, dc);
                });
            }
        }

        internal static bool IsBackgroundCheckRequired(ActivityType? participationObjectActivityType, int? webUserId)
        {
            // TODO: Blocked pending some way to approve background checks.
            // TODO: Check if Account wants to use background checks as not all will.
            // TODO: Check if Web User has a valid background check with a completed result.
            return false;
        }

        public static void AddESignature(this Registration registration, string esignature, string userAgent,
            string ipAddress, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            registration.ESignature = esignature;
            registration.ESignatureUserAgent = userAgent;
            registration.ESignatureIPAddress = ipAddress;
            registration.CompletionStatusId = RegistrationCompletionStatus.Signed;
            registration.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            registration.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(registration.Save(dc));
        }

        public static void SavePaymentSource(this Registration registration, string stripeSourceId,
            PaymentType paymentType, decimal paymentAmount, decimal convenienceFeePaid, decimal paymentTypeFeePaid,
            Account convenienceFeePayer, Account paymentTypeFeePayer, Coupon coupon, WebUser loggedInUser,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            registration.StripeSourceId = stripeSourceId;
            registration.PaymentTypeId = paymentType;
            registration.CostPerClass = paymentAmount;
            registration.LastModifiedDate = DateTime.UtcNow;
            registration.LastModifiedBy = loggedInUser?.ID;
            registration.ConvenienceFeePaid = convenienceFeePaid;
            registration.PaymentTypeFeePaid = paymentTypeFeePaid;
            registration.ConvenienceFeePayerAccountId = convenienceFeePayer.ID;
            registration.PaymentTypeFeePayerAccountId = paymentTypeFeePayer.ID;

            if (coupon.IsNotEmpty())
            {
                registration.CouponId = coupon.ID;
                registration.CouponValue = coupon.GetValue(paymentAmount - paymentTypeFeePaid);
            }

            msgs.AddRange(registration.Save(dc));
        }

        public static Account GetConvenienceFeePayer(this Registration registration, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Account.Load(registration.ConvenienceFeePayerAccountId.Value, msgs, dc);
        }

        public static Account GetPaymentTypeFeePayer(this Registration registration, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Account.Load(registration.PaymentTypeFeePayerAccountId.Value, msgs, dc);
        }

        public static List<SubscriptionPlanPrePayment> GetAvailablePrePayments(this Registration registration,
            Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlanPrePayment.LoadUnused(
                registration.ID.Value,
                activity.ID.Value,
                ParticipationObjectType.Activity,
                msgs,
                dc);
        }

        #region CSVReport

        public static void EmailCsvReport(int accountId, string searchText, int? questionId, List<int> participationObjectIds,
            ParticipationObjectType participationObjectType, string filename, RegistrationReportType reportType, List<RegistrationAssignmentType> rats,
            RegistrationStatus registrationStatus, string uiActivityLabel, string emails, string subject, string body, bool getVolunteers,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            msgs ??= new SystemMessages();

            var account = Account.Load(accountId, msgs, dc);

            var csvByteData = GetCsvByteArray(account, searchText, questionId, participationObjectIds, participationObjectType, reportType, rats,
                registrationStatus,
                getVolunteers, msgs, dc);

            EmailCsv(account, emails, subject, body, uiActivityLabel, CreateRegistrationsCsvFilename(filename), msgs, csvByteData);
        }

        public static byte[] GetCsvByteArray(Account account, string searchText, int? questionId, List<int> participationObjectIds,
            ParticipationObjectType participationObjectType, RegistrationReportType reportType, List<RegistrationAssignmentType> rats,
            RegistrationStatus registrationStatus, bool getVolunteers, SystemMessages msgs, DatabaseConnection dc)
        {
            var registrations = reportType == RegistrationReportType.DismissalForHostedActivities
                ? AccountService.SearchRegistrationsByActivityIds(participationObjectIds, msgs, dc)
                : account.SearchRegistrations(searchText, questionId, participationObjectIds, participationObjectType, registrationStatus, rats, null, null,
                    getVolunteers, msgs, dc);

            var csvText = ExportCsvText(registrations, account, reportType, registrationStatus, participationObjectIds, participationObjectType, rats, getVolunteers, msgs,
                dc);

            return Encoding.UTF8.GetBytes(csvText);
        }

        private static void EmailCsv(Account account, string emails, string subject, string body,
            string uiActivityLabel, string filename, SystemMessages msgs, byte[] byteData)
        {
            if (subject.IsBlank())
                subject = uiActivityLabel + " Report";

            if (body.IsBlank())
                body = string.Empty;

            body +=
                $"<p>Your {uiActivityLabel} report is attached.</p>{Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "student_report")}";

            var emailList = new List<string>();

            // add account email address
            if (account.Email.IsNotBlank())
                emailList.Add(account.Email);

            // get additional emails the user enters
            if (emails.IsNotBlank())
                emailList.AddRange(
                    emails.Split(',')
                        .Select(x => x.Trim())
                        .Where(email => email.IsEmail() && email.NotIn(emailList)));

            // send to email list
            emailList.ForEach(email =>
            {
                msgs.AddRange(SendGridHelper.SendMailWithAttachments(
                    fromEmail: new EmailAddress(
                        Email.AppNoReplyEmailAddress,
                        $"{account.Name} via {Utilities.AppName(account.IsAfterSchool)}"
                    ),
                    toEmail: new EmailAddress(email),
                    subject: subject,
                    body: body,
                    emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool),
                    attachmentByteData: byteData,
                    attachmentFilename: filename
                ));
            });
        }

        internal static string CreateRegistrationsCsvFilename(string userProposedFilename)
        {
            if (userProposedFilename.IsEmpty())
                return "registrations.csv";

            var newFilename = new Regex(@"[^\w_\-. ]").Replace(userProposedFilename, string.Empty);

            return newFilename.EndsWith(".csv") ? newFilename : newFilename + ".csv";
        }

        private static string ExportCsvText(List<Registration> registrations, Account account,
            RegistrationReportType reportType, RegistrationStatus registrationStatus, List<int> participationObjectIds,
            ParticipationObjectType participationObjectType, List<RegistrationAssignmentType> assignmentTypes,bool volunteerReport,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var allRegistrations = new List<Registration>();

            allRegistrations.AddRange(registrations);

            registrations.ForEach(registration =>
                allRegistrations.AddRange(registration.GetRelatedRegistrations(msgs, dc))
            );

            var registrantIds = allRegistrations.Select(x => x.RegistrantId.Value).ToList();
            var registrationIds = allRegistrations.Select(x => x.ID.Value).ToList();

            var registrantActivities = RegistrantActivity.LoadByRegistrantIds(registrantIds, msgs, dc);

            if (participationObjectIds.Any())
                registrantActivities = registrantActivities.Where(ra =>
                        ra.ParticipationObjectId.Value.In(participationObjectIds) &&
                        ra.ParticipationObjectTypeId == participationObjectType)
                    .ToList();

            if (assignmentTypes.IsNotEmpty() && assignmentTypes.Count > 0)
                registrantActivities = registrantActivities.Where(x => x.AssignmentTypeId.In(assignmentTypes))
                    .ToList();

            switch (registrationStatus)
            {
                case RegistrationStatus.Current:
                    registrantActivities = registrantActivities.Where(x => x.IsInSession(msgs, dc)).ToList();
                    break;
                case RegistrationStatus.Past:
                    registrantActivities = registrantActivities.Where(x => x.WasInSession(msgs, dc)).ToList();
                    break;
                case RegistrationStatus.Future:
                    registrantActivities = registrantActivities.Where(x => x.WillBeInSession(msgs, dc)).ToList();
                    break;
            }

            var activitySchedules = new List<ActivitySchedules>();

            if (reportType == RegistrationReportType.Dismissal || reportType == RegistrationReportType.DismissalForHostedActivities)
                registrantActivities.DistinctBy(ra => new {ra.ParticipationObjectId, ra.ParticipationObjectTypeId})
                    .ToList().ForEach(ra =>
                    {
                        var daysOfTheWeek = ra.GetSchedule(msgs, dc)?.GetDaysOfTheWeek(msgs, dc) ?? new List<DayOfWeekASL>();
                        activitySchedules.Add(new ActivitySchedules
                        {
                            DaysOfTheWeek = daysOfTheWeek,
                            ParticipationObjectId = ra.ParticipationObjectId.Value,
                            ParticipationObjectTypeId = ra.ParticipationObjectTypeId,
                        });
                    });

            var answers = FormQuestionAnswer.LoadByRegistrationIds(registrationIds, msgs, dc)
                .Where(x => x.Text.IsNotBlank())
                .ToList();

            var questionIds = answers.Select(x => x.FormQuestionId.Value).Distinct().ToList();

            var questions = FormQuestion.LoadList(questionIds, msgs, dc)
                .OrderBy(x => x.FormQuestionGroupDisplayOrder)
                .ThenBy(x => x.DisplayOrder)
                .ToList();

            var standardQuestions = questions
                .Where(x => x.QuestionDataTypeId != QuestionDataType.NotSet)
                .DistinctBy(x => x.QuestionDataTypeId)
                .ToList();

            var customQuestions = questions
                .Where(x => x.QuestionDataTypeId == QuestionDataType.NotSet)
                .DistinctBy(x => x.Text)
                .ToList();

            var table = new List<List<string>>
            {
                AddHeadersToCsvReport(reportType, standardQuestions, customQuestions, volunteerReport),
            };

            // print data for each registration
            registrations.ForEach(firstRegistration =>
            {
                var reportRegistration = new ReportRegistration(firstRegistration, allRegistrations, answers,
                    registrantActivities, activitySchedules);

                table.Add(AddEachRegistrationToCsvReport(
                    reportRegistration,
                    account,
                    reportType,
                    standardQuestions,
                    customQuestions,
                    volunteerReport,
                    msgs,
                    dc));
            });

            return CsvFile.Create(table);
        }

        private static List<string> AddHeadersToCsvReport(RegistrationReportType reportType, List<FormQuestion> standardQuestions,
            List<FormQuestion> customQuestions, bool volunteerReport)
        {
            var row = new List<string>
            {
                "Date Registered",
                "First Name",
                "Last Name",
                "Email",
            };

            if (reportType != RegistrationReportType.Dismissal)
            {
                if (volunteerReport)
                {
                    row.Add("Opportunities");
                    row.Add("Registration Status");
                }
                else
                {
                    row.Add("Activities");    
                }
            }

            if (reportType == RegistrationReportType.Dismissal || reportType == RegistrationReportType.DismissalForHostedActivities)
            {
                Enum.GetValues(typeof(DayOfWeekASL)).Cast<DayOfWeekASL>().Except(DayOfWeekASL.NotSet.AsList()).ToList()
                    .ForEach(dayOfWeek => { row.Add(dayOfWeek.ToString()); });
            }

            if (reportType != RegistrationReportType.Basic)
            {
                if(!volunteerReport)
                    row.Add("Amount Paid");
                AddQuestionHeadersToCSVReport(row, standardQuestions, customQuestions);
            }

            return row;
        }

        private class ActivitySchedules
        {
            public int ParticipationObjectId { get; set; }
            public ParticipationObjectType ParticipationObjectTypeId { get; set; }
            public List<DayOfWeekASL> DaysOfTheWeek { get; set; }

            public bool IsMatch(RegistrantActivity ra)
            {
                return ra.ParticipationObjectId == ParticipationObjectId &&
                       ra.ParticipationObjectTypeId == ParticipationObjectTypeId;
            }

            public bool HasMatch(List<RegistrantActivity> registrantActivities)
            {
                return registrantActivities.Any(ra => IsMatch(ra));
            }

            public List<RegistrantActivity> GetMatches(List<RegistrantActivity> registrantActivities)
            {
                return registrantActivities.Where(ra => IsMatch(ra)).ToList();
            }
        }

        private class ReportRegistration
        {
            public ReportRegistration(Registration firstRegistration, List<Registration> allRegistrations,
                List<FormQuestionAnswer> answers, List<RegistrantActivity> registrantActivities,
                List<ActivitySchedules> activitySchedules)
            {
                FirstRegistration = firstRegistration;
                RelatedRegistrations = allRegistrations.Where(reg => reg.FirstRegistrationId == FirstRegistration.ID)
                    .ToList();
                Answers = answers.Where(answer => answer.RegistrantId.Value.In(RegistrantIds)).ToList();
                RegistrantActivities =
                    registrantActivities.Where(ra => ra.RegistrantId.Value.In(RegistrantIds)).ToList();
                ActivitySchedules = activitySchedules.Where(schedules => schedules.HasMatch(RegistrantActivities))
                    .ToList();
            }

            public Registration FirstRegistration { get; set; }
            public List<Registration> RelatedRegistrations { get; set; }
            public List<RegistrantActivity> RegistrantActivities { get; set; }
            public List<FormQuestionAnswer> Answers { get; set; }
            public List<ActivitySchedules> ActivitySchedules { get; set; }

            public List<int> RegistrantIds
            {
                get { return AllRegistrations.Select(registration => registration.RegistrantId.Value).ToList(); }
            }

            public List<Registration> AllRegistrations
            {
                get
                {
                    var registrations = FirstRegistration.AsList();
                    registrations.AddRange(RelatedRegistrations);
                    return registrations;
                }
            }

            public List<int> AllRegistrationIds
            {
                get { return AllRegistrations.Select(registration => registration.ID.Value).ToList(); }
            }

            public decimal GetAmountPaid(SystemMessages msgs, DatabaseConnection dc = null)
            {
                return AllRegistrations.Sum(registration => registration.GetAmountPaid(msgs, dc));
            }

            public FormQuestionAnswer GetAnswer(FormQuestion question)
            {
                if (question.QuestionDataTypeId != QuestionDataType.NotSet)
                    return Answers
                        .Where(item =>
                            item.RegistrationId.Value.In(AllRegistrationIds) &&
                            item.FormQuestionQuestionDataTypeId == question.QuestionDataTypeId)
                        .OrderBy(x => x.RegistrationId)
                        .LastOrDefault();

                return Answers
                    .Where(item =>
                        item.RegistrationId.Value.In(AllRegistrationIds) &&
                        item.FormQuestionText.IsSameAs(question.Text))
                    .OrderBy(x => x.RegistrationId)
                    .LastOrDefault();
            }

            public List<RegistrantActivity> GetReservedActivitiesByDayOfTheWeek(DayOfWeekASL day)
            {
                var schedules = ActivitySchedules.Where(schedule => schedule.DaysOfTheWeek.Contains(day)).ToList();
                var ras = new List<RegistrantActivity>();
                var reservedRas = GetReservedActivities();

                schedules.ForEach(schedule => ras.AddRange(schedule.GetMatches(reservedRas)));

                return ras;
            }

            public List<RegistrantActivity> GetReservedActivities()
            {
                return RegistrantActivities.Where(ra => ra.AssignmentTypeId.In(Registration.ReservedStatuses)).ToList();
            }
        }

        private static List<string> AddEachRegistrationToCsvReport(ReportRegistration registration, Account account,
            RegistrationReportType reportType, List<FormQuestion> standardQuestions, List<FormQuestion> customQuestions,bool volunteerReport,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            var row = new List<string>
            {
                account.ConvertToAccountTimeZone(registration.FirstRegistration.CreateDate.Value).ToShortDateTimeString(),
                registration.FirstRegistration.RegistrantFirstName,
                registration.FirstRegistration.RegistrantLastName,
                registration.FirstRegistration.RegistrantEmail,
            };

            AddActivitiesToCsvReport(row, registration, reportType, msgs, dc);

            if (reportType == RegistrationReportType.Basic)
                return row;
            row.Add(volunteerReport ? registration.RegistrantActivities.FirstOrDefault()!.AssignmentTypeId.ToString() : registration.GetAmountPaid(msgs, dc).ToCurrency());
            AddQuestionsToCSVReport(row, standardQuestions, customQuestions, registration);

            return row;
        }

        private static void AddActivitiesToCsvReport(List<string> row, ReportRegistration registration,
            RegistrationReportType reportType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var ras = registration.RegistrantActivities;

            if (reportType != RegistrationReportType.Dismissal)
            {
                AddActivitiesToRow(row, ras, RegistrationReportType.Basic, msgs, dc);
            }

            if (reportType == RegistrationReportType.Dismissal || reportType == RegistrationReportType.DismissalForHostedActivities)
            {
                Enum.GetValues(typeof(DayOfWeekASL)).Cast<DayOfWeekASL>().Except(DayOfWeekASL.NotSet.AsList()).ToList()
                    .ForEach(dayOfWeek =>
                    {
                        var dayActivities = registration.GetReservedActivitiesByDayOfTheWeek(dayOfWeek);

                        AddActivitiesToRow(row, dayActivities, reportType, msgs, dc);
                    });
            }
        }

        private static void AddActivitiesToRow(List<string> row, List<RegistrantActivity> ras,
            RegistrationReportType reportType, SystemMessages msgs, DatabaseConnection dc)
        {
            var activitiesList = new List<string>();
            ras.ForEach(ra =>
            {
                var name = GetActivityNameForReport(ra, msgs, dc);

                if (reportType != RegistrationReportType.Dismissal)
                    name += $" - {ra.AssignmentTypeId.ToString().SplitByUpperCase()}";

                activitiesList.Add(name);
            });

            row.Add(activitiesList.ToStringList("; "));
        }

        private static string GetActivityNameForReport(RegistrantActivity registrantActivity, SystemMessages msgs,
            DatabaseConnection dc)
        {
            switch (registrantActivity.ParticipationObjectTypeId)
            {
                case ParticipationObjectType.AccountActivityGroup:
                    var aag = AccountActivityGroup.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc);
                    return ActivityGroup.Load(aag.ActivityGroupId.Value, msgs, dc).Name;
                case ParticipationObjectType.Activity:
                    return Activity.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc).Name;
                case ParticipationObjectType.ActivityGroup:
                    return ActivityGroup.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc).Name;
            }

            return "";
        }

        private static void AddQuestionsToCSVReport(List<string> row, List<FormQuestion> standardQuestions,
            List<FormQuestion> customQuestions, ReportRegistration registration)
        {
            foreach (var question in standardQuestions)
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantEmail:
                        break;
                    default:
                        var answer = registration.GetAnswer(question);

                        if (answer.IsNotEmpty())
                            ConvertJSONAnswersIntoText(answer, row);
                        else
                            row.Add("");
                        break;
                }
            }

            foreach (var question in customQuestions)
            {
                var answer = registration.GetAnswer(question);

                if (answer.IsNotEmpty())
                    ConvertJSONAnswersIntoText(answer, row);
                else
                    row.Add("");
            }
        }

        private static void ConvertJSONAnswersIntoText(FormQuestionAnswer answer, List<string> row)
        {
            if (answer.FormQuestionQuestionTypeId != QuestionType.RadioList &&
                answer.FormQuestionQuestionTypeId != QuestionType.DropDownList &&
                answer.FormQuestionQuestionTypeId != QuestionType.CheckboxList)
            {
                row.Add(answer.Text);
            }
            else if (answer.FormQuestionQuestionTypeId == QuestionType.Checkbox)
            {
                row.Add(answer.Text == "1" ? "True" : "False");
            }
            else if (answer.FormQuestionQuestionTypeId == QuestionType.DropDownList ||
                     answer.FormQuestionQuestionTypeId == QuestionType.RadioList)
            {
                dynamic json = JsonConvert.DeserializeObject(answer.Text);
                row.Add(Convert.ToString(json[0].Value));
            }
            else if (answer.FormQuestionQuestionTypeId == QuestionType.CheckboxList)
            {
                var str = "";
                dynamic json = JsonConvert.DeserializeObject(answer.Text);
                foreach (var item in json)
                {
                    if (str.IsNotEmpty())
                        str = str + ", " + item.value;
                    else
                        str = item.value;
                }

                row.Add(Convert.ToString(str));
            }
        }

        private static void AddQuestionHeadersToCSVReport(List<string> row, List<FormQuestion> standardQuestions,
            List<FormQuestion> customQuestions)
        {
            standardQuestions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                    case QuestionDataType.RegistrantEmail:
                        break;
                    default:
                        row.Add($"{question.FormQuestionGroupName} - {question.Text.ToString()}");
                        break;
                }
            });

            customQuestions.ForEach(question => row.Add(question.Text.ToString()));
        }

        public static ParticipationObject GetFirstParticipationObject(this Registration registration,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrantActivities = registration.GetRegistrantActivities(msgs, dc);
            if (registrantActivities.Any())
            {
                if (registrantActivities.Any(
                        ra => ra.ParticipationObjectTypeId == ParticipationObjectType.ActivityGroup))
                    return registrantActivities
                        .First(ra => ra.ParticipationObjectTypeId == ParticipationObjectType.ActivityGroup)
                        .GetParticipationObject(msgs, dc);

                return registrantActivities.First().GetParticipationObject(msgs, dc);
            }

            return null;
        }

        public static List<ParticipationObject> GetParticipationObjects(this Registration registration,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return registration.GetRegistrantActivities(msgs, dc)
                .Select(ra => ra.GetParticipationObject(msgs, dc))
                .ToList();
        }

        public static bool IsAlreadyPaidFor(this Registration registration, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var participationObject = registration.GetFirstParticipationObject(msgs, dc);
            var submittedRecord = registration.GetRegistrant(msgs, dc);
            var paidFee = submittedRecord.GetAmountPaid(msgs, dc) > 0;
            var hasSubscription = submittedRecord.GetActiveSubscription(participationObject, msgs, dc).IsNotEmpty();

            return paidFee && hasSubscription;
        }

        public static List<Registration> GetCurrentRelatedRegistrations(this Registration registration,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            var account = registration.GetAccount(msgs, dc);
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            var registrations = registration.GetRelatedRegistrations(msgs, dc);
            var finalList = new List<Registration>();
            var registrationDateRanges = new List<DateRange>();

            registration.GetParticipationObjects(msgs, dc)
                .Where(po => po.ObjectType == ParticipationObjectType.Activity).ToList().ForEach(po =>
                {
                    if (po.Schedule.IsNotEmpty())
                        registrationDateRanges.AddRange(po.Schedule.GetInstances(false, msgs, dc).Select(instance =>
                            new DateRange(instance.StartDateTime.Value, instance.EndDateTime.Value)));
                });

            registrations.ForEach(reg =>
            {
                var ras = reg.GetRegistrantActivities(msgs, dc);
                var activities = ras.Select(ra => ra.GetParticipationObject(msgs, dc))
                    .Where(po => po.ObjectType == ParticipationObjectType.Activity).ToList();

                activities.ForEach(activity =>
                {
                    if (activity.Schedule.IsNotEmpty())
                    {
                        var poDateRanges = activity.Schedule.GetInstances(false, msgs, dc).Select(instance =>
                            new DateRange(instance.StartDateTime.Value, instance.EndDateTime.Value));
                        if (DateRange.HasOverlappingDates(registrationDateRanges, poDateRanges))
                            finalList.Add(reg);
                    }
                });
            });

            return finalList;
        }

        public static Registrant GetRegistrant(this Registration registration, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Registrant.Load(registration.RegistrantId.Value, msgs, dc);
        }

        public static Registrant GetMasterRegistrant(this Registration registration, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var registrant = registration.GetRegistrant(msgs, dc);

            if (registrant.IsNotEmpty())
                return registrant.GetMasterRegistrant(msgs, dc);

            return null;
        }

        public static RegistrantActivity GetRegistrantActivity(this Registration registration,
            ParticipationObject participationObject, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return registration.GetRegistrantActivities(msgs, dc)
                .FirstOrDefault(x => x.ParticipationObjectId == participationObject.ObjectID &&
                                     x.ParticipationObjectTypeId == participationObject.ObjectType);
        }

        public static List<RegistrantActivity> GetRegistrantActivities(this Registration registration,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrantActivities = RegistrantActivity.LoadByRegistrationId(registration.ID.Value, msgs, dc);
            var participationObjectTypes = registrantActivities
                .Select(registrantActivity => registrantActivity.ParticipationObjectTypeId)
                .DistinctBy(registrantActivity => registrantActivity)
                .ToList();
            var rasToReturn = new List<RegistrantActivity>();

            participationObjectTypes.ForEach(participationObjectType =>
            {
                switch (participationObjectType)
                {
                    case ParticipationObjectType.Activity:
                        var activityIds = registrantActivities
                            .Where(x => x.ParticipationObjectTypeId == participationObjectType)
                            .Select(x => x.ParticipationObjectId.Value)
                            .ToList();
                        var activities = Activity.LoadList(activityIds, msgs, dc);
                        var validActivityIds = activities.Where(x =>
                                !x.IsDeleted &&
                                !x.IsArchived &&
                                (!x.ActivityGroupId.HasValue || !x.ActivityGroupIsArchived.Value))
                            .Select(activity => activity.ID.Value)
                            .ToList();

                        rasToReturn.AddRange(registrantActivities.Where(
                            registrantActivity =>
                                registrantActivity.ParticipationObjectTypeId == participationObjectType &&
                                registrantActivity.ParticipationObjectId.Value.In(validActivityIds)
                        ));

                        break;

                    case ParticipationObjectType.ActivityGroup:
                        var activityGroupIds = registrantActivities
                            .Where(x => x.ParticipationObjectTypeId == participationObjectType)
                            .Select(x => x.ParticipationObjectId.Value)
                            .ToList();
                        var activityGroups = ActivityGroup.LoadList(activityGroupIds, msgs, dc);
                        var validActivityGroupIds = activityGroups.Where(x => !x.IsArchived)
                            .Select(activity => activity.ID.Value);

                        rasToReturn.AddRange(registrantActivities.Where(
                            registrantActivity =>
                                registrantActivity.ParticipationObjectTypeId == participationObjectType &&
                                registrantActivity.ParticipationObjectId.Value.In(validActivityGroupIds)
                        ));

                        break;

                    case ParticipationObjectType.AccountActivityGroup:
                        var accountActivityGroupIds = registrantActivities
                            .Where(x => x.ParticipationObjectTypeId == participationObjectType)
                            .Select(x => x.ParticipationObjectId.Value)
                            .ToList();
                        var accountActivityGroups = AccountActivityGroup.LoadList(accountActivityGroupIds, msgs, dc);
                        var validAccountActivityGroupIds = accountActivityGroups.Where(x => !x.ActivityGroupIsArchived)
                            .Select(activity => activity.ID.Value);

                        rasToReturn.AddRange(registrantActivities.Where(
                            registrantActivity =>
                                registrantActivity.ParticipationObjectTypeId == participationObjectType &&
                                registrantActivity.ParticipationObjectId.Value.In(validAccountActivityGroupIds)
                        ));

                        break;
                }
            });

            return rasToReturn;
        }

        public static void RemoveParticipation(this Registration registration, int participationObjectId,
            ParticipationObjectType participationObjectType, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var ra = RegistrantActivity.LoadByParticipationObjectByRegistrant(
                participationObjectId,
                participationObjectType,
                registration.RegistrantId.Value,
                msgs,
                dc);

            if (ra.IsNotEmpty())
                ra.CancelRegistration(msgs, dc);
        }

        public static void WasUpdated(this Registration registration, SystemMessages msgs, DatabaseConnection dc = null)
        {
            registration.LastUpdated = DateTime.UtcNow;
            msgs.AddRange(registration.Save(dc: dc));
        }

        internal static void DeleteSessionActions(this Registration registration, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // delete all related session actions
                    SessionAction.LoadByRegistrationId(registration.ID.Value, amsgs, adc)
                        .ForEach(action => amsgs.AddRange(action.Delete(adc)));
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        internal static void DeleteAnswers(this Registration registration, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // delete all answers associated to this registration
                    registration.GetAnswers(amsgs, adc).ForEach(answer => amsgs.AddRange(answer.Delete(adc)));
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        internal static void UpdateRelatedRegistrationsAfterDelete(this Registration registration, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            // this may be the first registration of other registrations :(
            // and we're about to delete this registration, so we have to
            // switch the first registration id to a new one, 
            // which kind of sucks because we need to know when their real first reg was
            // TODO: Figure out way to keep the date of their first registration (MAYBE)
            // it's the first registration if FirstRegistrationId is null
            if (!registration.FirstRegistrationId.HasValue)
            {
                // get reg's where the first reg id is equal to the mergedfrom
                var laterRegs = Registration.LoadByFirstRegistrationId(registration.ID.Value, dc: dc);

                // reset each one
                foreach (var laterReg in laterRegs)
                {
                    // reset the first registration id
                    laterReg.FirstRegistrationId = null;
                    msgs.AddRange(laterReg.Save(dc: dc));

                    laterReg.FindAndSetFirstRegistration(laterReg.RegistrantFirstName, laterReg.RegistrantLastName,
                        laterReg.RegistrantEmail, msgs, dc);
                }
            }
        }

        public static void SetRegistrationStatus(this Registration registration,
            RegistrationCompletionStatus completionStatus, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            registration.CompletionStatusId = completionStatus;
            registration.LastModifiedDate = DateTime.UtcNow;
            registration.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            msgs.AddRange(registration.Save(dc));

            var firstName = registration.RegistrantFirstName;
            var lastName = registration.RegistrantLastName;
            var email = registration.RegistrantEmail;

            if (firstName == null || lastName == null || email == null)
            {
                var registrant = registration.GetRegistrant(msgs, dc);
                firstName = registrant.FirstName;
                lastName = registrant.LastName;
                email = registrant.Email;
            }

            registration.FindAndSetFirstRegistration(firstName, lastName, email, msgs, dc);
        }

        public static bool IsPaid(this Registration registration, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var registrantActivities = registration.GetRegistrantActivities(msgs, dc);

            if (registrantActivities.Any())
            {
                var registrant = registration.GetRegistrant(msgs, dc);
                // this only works now because we're doing on registration at a time
                var participationObject = registration.GetFirstParticipationObject(msgs, dc);
                var registrantActivity = registrantActivities.First();

                var paidFees = registrantActivity.GetAmountPaid(msgs, dc) > 0;
                var registrationFeeRequired = !participationObject.IsRegistrationFree;
                var requiresSubscription = participationObject.SubscriptionPlans.Any();
                var hasActiveSubscription = registrant.HasActiveSubscription(participationObject, msgs, dc);

                return (!registrationFeeRequired || paidFees) && (!requiresSubscription || hasActiveSubscription);
            }

            msgs.Add("Incomplete registration", MessageType.UserError);

            return false;
        }

        public static decimal GetAmountPaid(this Registration registration, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var registrantActivities = registration.GetRegistrantActivities(msgs, dc);

            if (registrantActivities.Any())
                return registrantActivities.First().GetAmountPaid(msgs, dc);

            return 0;
        }

        public static Purchase GetPurchase(this Registration registration, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (registration.PurchaseId.HasValue)
                return Purchase.Load(registration.PurchaseId.Value, msgs, dc);

            return null;
        }

        public static List<Registration> GetRelatedRegistrations(this Registration registration,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrations = new List<Registration>();
            if (registration.FirstRegistrationId.HasValue)
            {
                registrations.Add(Registration.Load(registration.FirstRegistrationId.Value, msgs, dc));
                registrations.AddRange(
                    Registration.LoadByFirstRegistrationId(registration.FirstRegistrationId.Value, msgs, dc));
            }
            else
            {
                registrations.AddRange(Registration.LoadByFirstRegistrationId(registration.ID.Value, msgs, dc));
            }

            return registrations.Where(x => x.ID != registration.ID).ToList();
        }

        public static List<FormQuestionAnswer> GetAnswers(this Registration registration, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return FormQuestionAnswer.LoadByRegistrationId(registration.ID.Value, msgs, dc);
        }

        public static void MoveActivity(this Registration registration, RegistrantActivity ra, int loggedInUserId,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            // delete previous relationships to old registration
            //msgs.AddRange(ra.DeleteRegistrationActivity(dc: dc));

            // move to new registrant and registration
            ra.RegistrantId = registration.RegistrantId;
            ra.RegistrationId = registration.ID;
            ra.LastModifiedBy = loggedInUserId;
            ra.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(ra.Save(dc));

            // add to this registration record 
            // TODO: hopefully will remove this in the future, the whole registration activity object :(
            //this.AddActivity(ra.ActivityId.Value, loggedInUserId, msgs, dc);
        }

        public static Account GetAccount(this Registration registration, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return Account.Load(registration.AccountId.Value, msgs, dc);
        }

        public static ActivityGroup GetActivityGroup(this Registration registration, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (registration.ActivityGroupId.HasValue)
                return ActivityGroup.Load(registration.ActivityGroupId.Value, msgs, dc);

            return null;
        }

        #endregion
    }
}