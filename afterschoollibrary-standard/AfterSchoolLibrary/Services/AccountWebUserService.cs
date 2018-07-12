using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;

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
    }
}
