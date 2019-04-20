using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class RegistrationService
    {
        #region CSVReport

        public static SystemMessages EmailCSVReport(int accountId, string searchText, int? questionId, List<int> participationObjectIds, ParticipationObjectType participationObjectType, string filename, RegistrationReportType reportType, List<RegistrationAssignmentType> rats, RegistrationStatus registrationStatus, string uiActivityLabel, string emails, string subject, string body)
        {
            var msgs = new SystemMessages();

            var account = Account.Load(accountId);

            var registrations = account.SearchRegistrations(
                searchText,
                questionId,
                participationObjectIds,
                participationObjectType,
                registrationStatus,
                rats,
                null,
                null);

            var csv = RegistrationService.ExportCSV(
                registrations,
                account,
                reportType,
                registrationStatus,
                rats);

            var fileLink = RegistrationService.UploadCSV(account, filename, csv, msgs);

            EmailCSV(account, emails, subject, body, uiActivityLabel, fileLink, msgs);

            return msgs;
        }

        private static void EmailCSV(Account account, string emails, string subject, string body, string uiActivityLabel, string fileLink, SystemMessages msgs)
        {
            if (subject.IsBlank())
                subject = uiActivityLabel + " Report";

            if (body.IsBlank())
                body = "";

            body += $"<p>Here is your {uiActivityLabel} report: <a href=\"{fileLink}\">{fileLink}</a></p>{(Helpers.Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "student_report"))}";

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
                msgs.AddRange(SendGridHelper.SendMail(
                    fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                        Helpers.Email.AppEmailAddress,
                        $"{account.Name} via {Helpers.Utilities.AppName(account.IsAfterSchool)}"
                        ),
                    toEmail: new SendGrid.Helpers.Mail.EmailAddress(email),
                    subject: subject,
                    body: body,
                    emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                ))
            );
        }

        private static string UploadCSV(Account account, string filename, string csv, SystemMessages msgs)
        {
            var ms = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(csv));
            var ah = new AmazonHelper(
                Helpers.Utilities.AmazonBucketName,
                Helpers.Utilities.AmazonAccessKeyId,
                Helpers.Utilities.AmazonSecretAccessKeyId,
                Helpers.Utilities.AmazonRegionEndPoint);

            if (filename.IsNotEmpty())
            {
                filename = new System.Text.RegularExpressions.Regex(BasicLibrary.Utilities.NoAlphaNumericCharactersRegEx)
                    .Replace(filename, "");

                if (!filename.Contains(".csv"))
                    filename += ".csv";
            }
            else
                filename = "registrations.csv";

            string amazonLocation = $"{account.Uid.ToString()}/{Guid.NewGuid().ToString()}/{filename}";
            msgs.AddRange(ah.AddFile(ms, amazonLocation, true));

            return ah.GetFileLink(amazonLocation, false, null, true);
        }

        public static string ExportCSV(List<Registration> registrations, Account account, RegistrationReportType reportType, RegistrationStatus registrationStatus, List<RegistrationAssignmentType> assignmentTypes, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrantActivities = new List<RegistrantActivity>();
            var registrantIds = registrations.Select(x => x.RegistrantId.Value).ToList();

            registrantActivities = RegistrantActivity.LoadByRegistrantIds(registrantIds, msgs, dc);

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

            var answers = FormQuestionAnswer.LoadByRegistrationIds(registrations.Select(x => x.ID.Value).ToList(), msgs, dc)
               .Where(x => x.Text.IsNotBlank())
               .ToList();

            var questionIds = answers.Select(x => x.FormQuestionId.Value).Distinct().ToList();

            var questions = FormQuestion.LoadList(questionIds, msgs, dc)
                .OrderBy(x => x.DisplayOrder)
                .OrderBy(x => x.FormQuestionGroupDisplayOrder)
                .ToList();

            var standardQuestions = questions
                .Where(x => x.QuestionDataTypeId != QuestionDataType.NotSet)
                .DistinctBy(x => x.QuestionDataTypeId)
                .ToList();

            var customQuestions = questions
                .Where(x => x.QuestionDataTypeId == QuestionDataType.NotSet)
                .DistinctBy(x => x.Text)
                .ToList();

            var table = new List<List<string>>();

            table.Add(AddHeadersToCSVReport(
                reportType,
                standardQuestions,
                customQuestions
                ));

            // print data for each registration
            registrations.ForEach(registration =>
                table.Add(AddEachRegistrationToCSVReport(
                    registration,
                    account,
                    reportType,
                    registrantActivities,
                    standardQuestions,
                    customQuestions,
                    answers,
                    msgs,
                    dc))
            );

            return CsvFile.Create(table);
        }

        private static List<string> AddHeadersToCSVReport(RegistrationReportType reportType, List<FormQuestion> standardQuestions, List<FormQuestion> customQuestions)
        {
            var row = new List<string>();

            row.Add($"Date Registered");

            row.Add("First Name");
            row.Add("Last Name");
            row.Add("Email");
            row.Add("Activities");

            if (reportType != RegistrationReportType.Basic)
            {
                row.Add("Amount Paid");
                AddQuestionHeadersToCSVReport(row, standardQuestions, customQuestions);
            }

            return row;
        }

        private static List<string> AddEachRegistrationToCSVReport(Registration registration, Account account, RegistrationReportType reportType, List<RegistrantActivity> registrantActivities, List<FormQuestion> standardQuestions, List<FormQuestion> customQuestions, List<FormQuestionAnswer> answers, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var row = new List<string>();

            row.Add(account.ConvertToAccountTimeZone(registration.CreateDate.Value).ToShortDateTimeString());

            row.Add(registration.RegistrantFirstName);
            row.Add(registration.RegistrantLastName);
            row.Add(registration.RegistrantEmail);

            AddActivitiesToCSVReport(row, registration, registrantActivities);

            if (reportType != RegistrationReportType.Basic)
            {
                row.Add(registration.GetAmountPaid(msgs, dc).ToCurrency());
                AddQuestionsToCSVReport(row, standardQuestions, customQuestions, answers, registration);
            }

            return row;
        }

        private static void AddActivitiesToCSVReport(List<string> row, Registration registration, List<RegistrantActivity> registrantActivities, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var regIds = registration.ID.Value.AsList();
            var ras = registrantActivities.Where(registrantActivity => registrantActivity.RegistrationId.Value.In(regIds))
                .ToList();
            var participationObjects = ras.Select(registrantActivity =>
                new
                {
                    registrantActivity.ParticipationObjectId,
                    registrantActivity.ParticipationObjectTypeId,
                    registrantActivity.AssignmentTypeId
                })
                .ToList();

            var activitiesList = new List<string>();
            participationObjects.ForEach(participationObject => 
            {
                var name = "";

                switch (participationObject.ParticipationObjectTypeId)
                {
                    case ParticipationObjectType.AccountActivityGroup:
                        var aag = AccountActivityGroup.Load(participationObject.ParticipationObjectId.Value, msgs, dc);
                        name = ActivityGroup.Load(aag.ActivityGroupId.Value, msgs, dc).Name;
                        break;
                    case ParticipationObjectType.Activity:
                        name = Activity.Load(participationObject.ParticipationObjectId.Value, msgs, dc).Name;
                        break;
                    case ParticipationObjectType.ActivityGroup:
                        name = ActivityGroup.Load(participationObject.ParticipationObjectId.Value, msgs, dc).Name;
                        break;
                }

                activitiesList.Add($"{name} - {participationObject.AssignmentTypeId.ToString().SplitByUpperCase()}");
            });            

            row.Add(activitiesList.ToStringList("; "));
        }

        private static void AddQuestionsToCSVReport(List<string> row, List<FormQuestion> standardQuestions, List<FormQuestion> customQuestions, List<FormQuestionAnswer> answers, Registration registration)
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
                        var answer = answers
                            .Where(item =>
                                registration.ID == item.RegistrationId &&
                                item.FormQuestionQuestionDataTypeId == question.QuestionDataTypeId)
                            .OrderBy(x => x.RegistrationId)
                            .LastOrDefault();

                        if (answer.IsNotEmpty())
                            row.Add(answer.Text);
                        else
                            row.Add("");
                        break;
                }
            }

            foreach (var question in customQuestions)
            {
                var answer = answers
                    .Where(item =>
                        registration.ID == item.RegistrationId &&
                        item.FormQuestionText == question.Text)
                    .OrderBy(x => x.RegistrationId)
                    .LastOrDefault();

                if (answer.IsNotEmpty())
                    row.Add(answer.Text);
                else
                    row.Add("");
            }
        }

        private static void AddQuestionHeadersToCSVReport(List<string> row, List<FormQuestion> standardQuestions, List<FormQuestion> customQuestions)
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

        public static ParticipationObject GetParticipationObject(this Registration registration, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrantActivities = registration.GetRegistrantActivities(msgs, dc);
            if (registrantActivities.Any())
            {
                var registrantActivity = registrantActivities.First();

                switch (registrantActivity.ParticipationObjectTypeId)
                {
                    case ParticipationObjectType.AccountActivityGroup:
                        return AccountActivityGroup.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc)
                            .GetParticipationObject(msgs, dc);
                    case ParticipationObjectType.ActivityGroup:
                        return ActivityGroup.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc)
                            .GetParticipationObject(msgs, dc);
                    case ParticipationObjectType.Activity:
                        return Activity.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc)
                            .GetParticipationObject(msgs, dc);
                }
            }

            return null;
        }

        public static bool IsAlreadyPaidFor(this Registration registration, SystemMessages msgs = null)
        {
            var purchase = registration.GetPurchase(msgs);

            if (purchase.IsNotEmpty())
                return Transaction.GetCountByPurchaseId(purchase.ID.Value) > 0;

            return false;
        }

        #endregion
    }
}
