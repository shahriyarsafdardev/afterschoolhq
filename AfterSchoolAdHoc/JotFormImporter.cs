using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using File = System.IO.File;
using System.Text;
using CsvHelper;
using System.Globalization;

namespace AfterSchoolAdHoc
{
    internal class JotFormImporter
    {
        public static void Run(string filePath, SystemMessages msgs, DatabaseConnection dc)
        {           
            Database.ExecuteTransaction
            (
                action: (amsgs, adc) =>
                {
                    ProviderJotFormSubmission.Create(adc);
                    ProviderJotFormSubmission.Alter(adc);

                    using (var reader = new StreamReader(filePath))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        var submissions = csv.GetRecords<ProviderJotFormSubmissionDTO>();

                        foreach(var submission in submissions)
                        {
                            if (submission.SubmissionId.IsNotBlank() && !ProviderJotFormSubmission.Exists(submission.SubmissionId, amsgs, adc))
                            {
                                var pjfs = new ProviderJotFormSubmission()
                                {
                                    AnalyticsDashboard = submission.AnalyticsDashboard,
                                    BestEmail = submission.BestEmail,
                                    BestWayToReach = submission.BestWayToReach,
                                    CurrentlyUsing = submission.CurrentlyUsing,
                                    ForProfit = submission.ForProfit,
                                    Have15Minutes = submission.Have15Minutes,
                                    HowToLearnMore = submission.HowToLearnMore,
                                    ImplementationTimeline = submission.ImplementationTimeline,
                                    NextRegistrationPeriod = submission.NextRegistrationPeriod,
                                    OtherDecisionMakers = submission.OtherDecisionMakers,
                                    PainPoints = submission.PainPoints,
                                    PhoneNumber = submission.PhoneNumber,
                                    ProgramLocation = submission.ProgramLocation,
                                    SubmissionDate = submission.SubmissionDate.ToDateTime(),
                                    TeamMembers = submission.TeamMembers,
                                    WantDemo = submission.WantDemo,
                                    WantProfile = submission.WantProfile,
                                    WhatsNext = submission.WhatsNext,
                                    WithinBudget = submission.WithinBudget,
                                    SubmissionId = submission.SubmissionId,
                                };

                                amsgs.AddRange(pjfs.Save(adc));
                            }
                        }
                    }
                },
                onSuccess: (smsgs) => msgs.AddRange(smsgs),
                onFailure: (fmsgs) => msgs.AddRange(fmsgs),
                onException: (emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );            

        }

        public class ProviderJotFormSubmissionDTO
        {
            public string SubmissionDate { get; set; }
            public string TeamMembers { get; set; }
            public string ForProfit { get; set; }
            public string CurrentlyUsing { get; set; }
            public string AnalyticsDashboard { get; set; }
            public string ProgramLocation { get; set; }
            public string ImplementationTimeline { get; set; }
            public string WithinBudget { get; set; }
            public string PainPoints { get; set; }
            public string PhoneNumber { get; set; }
            public string WantProfile { get; set; }
            public string WantDemo { get; set; }
            public string BestEmail { get; set; }
            public string OtherDecisionMakers { get; set; }
            public string NextRegistrationPeriod { get; set; }
            public string Have15Minutes { get; set; }
            public string WhatsNext { get; set; }
            public string BestWayToReach { get; set; }
            public string HowToLearnMore { get; set; }
            public string SubmissionId { get; set; }
        }

    }
}
