using AfterSchoolLibrary.Services;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AfterSchool
{
    public class AppConfig
    {
        public static void Init()
        {
            RollbarSharp.Configuration.DefaultEnvironment = AfterSchoolLibrary.Helpers.Utilities.GetRollbarEnvironment();

            AfterSchoolLibrary.Helpers.Utilities.InitializeApp();

            // schedule and run the task daily at 3am EST (already installed)
            RecurringJob.AddOrUpdate(() => AccountService.PreparePayouts(), Cron.Daily(hour: 3, minute: 0));
        }
    }
}
