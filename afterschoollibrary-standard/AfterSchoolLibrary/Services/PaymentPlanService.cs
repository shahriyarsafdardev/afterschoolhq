//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BasicLibrary;
//using BasicLibrary.DataAccess;

//namespace AfterSchoolLibrary.Services
//{
//    public class PaymentPlanService
//    {
//        public static PaymentPlan CreateNew(Account planOwner, Activity activity, decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
//        {
//            return CreateNew(planOwner, ParticipationObjectType.Activity, activity.ID.Value, costPerPeriod, billingPeriodType, totalPeriods, loggedInUser?.ID.IfEmpty(0), msgs, dc);
//        }

//        public static PaymentPlan CreateNew(Account planOwner, ActivityGroup activityGroup, decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
//        {
//            return CreateNew(planOwner, ParticipationObjectType.ActivityGroup, activityGroup.ID.Value, costPerPeriod, billingPeriodType, totalPeriods, loggedInUser?.ID.IfEmpty(0), msgs, dc);
//        }

//        public static PaymentPlan CreateNew(Account planOwner, ParticipationObjectType participationObjectType, int participationObjectId, decimal costPerPeriod, BillingPeriodType billingPeriodType, int totalPeriods, int? loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
//        {
//            if (msgs.IsEmpty()) msgs = new SystemMessages();

//            var plan = new PaymentPlan()
//            {
//                PlanOwnerAccountId = planOwner.ID,
//                BillingPeriodTypeId = billingPeriodType,
//                CostPerPeriod = costPerPeriod,
//                CreateDate = DateTime.UtcNow,
//                CreatedBy = loggedInUserId,
//                LastModifiedDate = DateTime.UtcNow,
//                LastModifiedBy = loggedInUserId,
//                IsActive = true,
//                ParticipationObjectId = participationObjectId,
//                ParticipationObjectTypeId = participationObjectType,
//                TotalPeriods = totalPeriods
//            };

//            msgs.AddRange(plan.SaveWithStripe(dc));
            
//            return plan;
//        }
//    }
//}
