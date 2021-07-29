using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class SurveyRequestService
    {
        public static SurveyRequest CreateNewSurveyRequest(int formId, int activityId, int accountId, string name, string instrcutionsText, bool alloMultipleResponse, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            SurveyRequest surveyRequest = null;

            Database.ExecuteTransaction
            (
               action: (SystemMessages amsgs, DatabaseConnection adc) =>
               {
                   surveyRequest = new SurveyRequest()
                   {
                       FormId = formId,
                       ActivityId = activityId,
                       AccountId = accountId,
                       Name = name,
                       InstructionsText = instrcutionsText,
                       MultipleResponsesEnabled = alloMultipleResponse
                   };

                   msgs.AddRange(surveyRequest.Save(dc));
               },
               onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
               onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
               onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
               dc: dc
           );


            return surveyRequest;
        }


        public static void ExportSurveyRequestToCSV(List<string> headers, int id, SystemMessages msgs)
        {
            var table = new List<List<string>>();
            table.Add(AddHeadersToCSVReport(headers));

            var getSurveys = SurveyFeedback.LoadByActivityId(id, msgs);

            getSurveys.ForEach(firstRegistrant => {

                table.Add(AddRegistrantRow());
            });

            CsvFile.Create(table);
        }

        private static List<string> AddRegistrantRow()
        {
            throw new NotImplementedException();
        }

        public static List<string> AddHeadersToCSVReport(List<string> headers)
        {
            var row = new List<string>();
            foreach (var header in headers)
            {
                row.Add(header);
            }
            return row;
        }
    }
}
