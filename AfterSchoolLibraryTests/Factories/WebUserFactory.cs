using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;

namespace AfterSchoolLibraryTests.Factories
{
    public class WebUserFactory
    {
        public static WebUser Create(SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new WebUser() { };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static WebUser Create(bool isActive, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new WebUser() { IsActive = isActive };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static WebUser CreateWithEmail(string email, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new WebUser() { Email = email };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static WebUser CreateWithEmail(string email, string password, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new WebUser() { Email = email, Password = WebUser.EncryptPassword(password) };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static WebUser CreateWithUsername(string username, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new WebUser() { Username = username };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }

        public static WebUser CreateWithUsername(string username, string password, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new WebUser() { Username = username, Password = WebUser.EncryptPassword(password) };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
