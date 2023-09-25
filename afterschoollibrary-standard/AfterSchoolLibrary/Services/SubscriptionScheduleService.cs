using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AfterSchoolLibrary.Services
{
    public static class SubscriptionScheduleService
    {
        public static List<SubscriptionSchedule> GetSubscriptionSchedulePendingToday(SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            var subscriptionSchedulesNotCancelled = SubscriptionSchedule.LoadActiveSubscriptionSchedule(msgs, dc);
            var currentDate = DateTime.UtcNow.Date;
            var subscriptionSchedulespendingToday = subscriptionSchedulesNotCancelled
               .Where(x =>
                    x.NextBillDate.Value.Date.Equals(currentDate)
                )
                .ToList();

            return subscriptionSchedulespendingToday;
        }

        public static List<SubscriptionSchedule> GetSubscriptionSchedulePendingInThreeDays(SystemMessages msgs,
           DatabaseConnection dc = null)
        {
            var subscriptionSchedulesNotCancelled = SubscriptionSchedule.LoadActiveSubscriptionSchedule(msgs, dc);
            var date = DateTime.UtcNow.AddDays(3).Date;
            var subscriptionSchedules = subscriptionSchedulesNotCancelled
               .Where(x =>
                    x.NextBillDate.Value.Date.Equals(date) &&
                    (x.BillingPeriodType == BillingPeriodType.Month || x.BillingPeriodType == BillingPeriodType.Year)
                )
                .ToList();
            return subscriptionSchedules;
        }
    }
}
