using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class SessionFactory
    {
        public static Session Create(SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Session() { UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:79.0) Gecko/20100101 Firefox/79.0", UrlReferrer= "http://localhost:54849/" };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
