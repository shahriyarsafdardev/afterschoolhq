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
            var obj = new WebUser() { FirstName = "Don", LastName = "John", Email = "don@john.com" };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static WebUser Create(bool isActive, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new WebUser() { FirstName = "Don", LastName = "John", Email = "don@john.com", IsActive = isActive };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static WebUser CreateWithEmail(string email, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new WebUser() { FirstName = "Don", LastName = "John", Email = email };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static WebUser CreateWithEmail(string email, string password, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new WebUser() { FirstName = "Don", LastName = "John", Email = email, Password = WebUserService.EncryptPassword(password) };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static WebUser CreateWithUsername(string username, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new WebUser() { FirstName = "Don", LastName = "John", Email = "don@john.com", Username = username };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static WebUser CreateWithUsername(string username, string password, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new WebUser() { FirstName = "Don", LastName = "John", Email = "don@john.com", Username = username, Password = WebUserService.EncryptPassword(password) };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
