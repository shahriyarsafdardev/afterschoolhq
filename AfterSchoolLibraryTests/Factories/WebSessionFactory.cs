using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfterSchoolLibraryTests.Factories
{
    public class WebSessionFactory
    {
        public static WebSession Create(Account account, WebUser webUser, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new WebSession() { UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:79.0) Gecko/20100101 Firefox/79.0", UrlReferrer = "http://localhost:54849/", WebUserId = webUser.ID.Value, AccountId = account.ID.Value };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
