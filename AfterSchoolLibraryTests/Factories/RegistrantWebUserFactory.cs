using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class RegistrantWebUserFactory
    {
        public static RegistrantWebUser Create(Registrant registrant, WebUser user, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new RegistrantWebUser()
            {
                RegistrantId = registrant.ID,
                WebUserId = user.ID,
                IsDeleted = false,
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
