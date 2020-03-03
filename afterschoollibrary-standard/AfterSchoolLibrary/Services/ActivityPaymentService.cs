using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class ActivityPaymentService
    {
        public static void MarkAsDeleted(this ActivityPayment payment, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            payment.IsDeleted = true;
            payment.LastModifiedDate = DateTime.UtcNow;
            payment.LastModifiedBy = loggedInUser.ID;
            msgs.AddRange(payment.Save(dc));
        }
    }
}
