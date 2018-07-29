//using System;
//using System.Collections.Generic;
//using System.Text;
//using BasicLibrary;
//using BasicLibrary.DataAccess;

//namespace AfterSchoolLibrary.Services
//{
//    public static class RegistrantPaymentPlanService
//    {
//        public static RegistrantPaymentPlan CreateNew(Registrant registrant, PaymentPlan paymentPlan, DateTime startDate, int? purchaseId, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
//        {
//            if (msgs.IsEmpty()) msgs = new SystemMessages();

//            var rpp = new RegistrantPaymentPlan()
//            {
//                CreateDate = DateTime.UtcNow,
//                CreatedBy = loggedInUser?.ID.IfEmpty(0),
//                LastModifiedDate = DateTime.UtcNow,
//                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
//                PaymentPlanId = paymentPlan.ID,
//                RegistrantId = registrant.ID,
//                StartDate = startDate,
//                PurchaseId = purchaseId
//            };

//            msgs.AddRange(rpp.Save(dc));

//            return rpp;
//        }
//    }
//}
