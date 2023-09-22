using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class SubscriptionScheduleService
    {
        public static List<SubscriptionSchedule> GetSubscriptionSchedulePendingToday(SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            var subscriptionSchedulespendingToday = SubscriptionSchedule.LoadSubscriptionSchedulesPendingToday(DateTime.UtcNow, msgs, dc);
            return subscriptionSchedulespendingToday;
        }
    }
}
