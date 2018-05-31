using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BasicLibrary;
using AfterSchoolLibrary;

namespace AfterSchool.Models
{
    [Serializable]
    public class SessionAccount
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool IsAdminAccount { get; set; }
        public string UniqueName { get; set; }

        public SessionAccount(Account a)
        {
            ID = a.ID.Value;
            Name = a.Name;
            IsAdminAccount = a.IsAdminAccount;
            UniqueName = a.UniqueName;
        }

        public Account ToAccount(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return Account.Load(this.ID, dc);
        }
    }
}