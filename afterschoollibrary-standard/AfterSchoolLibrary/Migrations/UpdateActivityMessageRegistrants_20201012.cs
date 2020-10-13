using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;

namespace AfterSchoolLibrary.Migrations
{
    public class UpdateActivityMessageRegistrants_20201012
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    //For each ActivityMessageRegistrant record, please set the SentEmail to true, because all records have an email address
                    amsgs.AddRange(ActivityMessageRegistrant.Update(
                        updateValues: new Hashtable() { { "SentEmail", true } },
                        criterias: new Hashtable() { },
                        ObjectMap: ActivityMessageRegistrant.ObjectMap,
                        dc: adc));

                    //TODO: To see if a text message was sent for a registrant, you will need to check the TwilioSMS table to see if a record exists there and if the SMS was successful.
                    //var twlioSms = TwilioSMS.Load(
                    //    criterias: new Hashtable() { { "IsMaster", true } },
                    //    msgs: amsgs,
                    //    dc: adc);
                },
                uniqueIdenfier: "UpdateActivityMessageRegistrants-20201012",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
