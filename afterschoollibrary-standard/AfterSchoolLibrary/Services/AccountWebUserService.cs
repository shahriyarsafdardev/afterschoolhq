using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibrary.Services
{
    public static class AccountWebUserService
    {
        public static SystemMessages Activate(this AccountWebUser awu, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            awu.IsActive = true;
            awu.LastModifiedDate = DateTime.UtcNow;
            awu.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            return awu.Save(dc);
        }
        public static SystemMessages Deactivate(this AccountWebUser awu, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            awu.IsActive = false;
            awu.LastModifiedDate = DateTime.UtcNow;
            awu.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            return awu.Save(dc);
        }

        public static void MarkAsDeleted(this AccountWebUser awu, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            awu.IsDeleted = true;
            awu.LastModifiedDate = DateTime.UtcNow;
            awu.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            msgs.AddRange(awu.Save(dc));
        }

        public static void UpdateUserType(this AccountWebUser awu, WebUser loggedInUser, string usertype, SystemMessages msgs, DatabaseConnection dc = null)
        {
            awu.LastModifiedDate = DateTime.UtcNow;
            awu.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            awu.UserTypeId = usertype.ToEnum<UserType>();
            msgs.AddRange(awu.Save(dc));
        }

        public static Account GetAccount(this AccountWebUser awu, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Account.Load(awu.AccountId.Value, msgs, dc);
        }
    }
}
