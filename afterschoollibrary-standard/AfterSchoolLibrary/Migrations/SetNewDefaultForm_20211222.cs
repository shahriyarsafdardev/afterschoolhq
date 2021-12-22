using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Migrations
{
    public class SetNewDefaultForm_20211222
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    FormService.AddNewAfterSchoolDefaultForm(amsgs,adc);
                },
                uniqueIdenfier: "SetNewDefaultForm_20211222",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
