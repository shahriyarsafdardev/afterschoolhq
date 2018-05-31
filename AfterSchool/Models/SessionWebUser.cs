using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;

namespace AfterSchool.Models
{
    [Serializable]
    public class SessionWebUser
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public Guid Uid { get; set; }

        public SessionWebUser(WebUser user)
        {
            ID = user.ID.Value;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Username = user.Username;
            Email = user.Email;
            Uid = user.Uid;
        }

        public WebUser ToWebUser(DatabaseConnection dc = null)
        {
            return WebUser.Load(this.ID, dc);
        }

        public SystemMessages UpdateLastActivity(DatabaseConnection dc = null)
        {
            return WebUser.UpdateLastActivityById(this.ID);
        }

        public SystemMessages UpdateLastLogin(DatabaseConnection dc = null)
        {
            return WebUser.UpdateLastLoginById(this.ID);
        }
    }
}