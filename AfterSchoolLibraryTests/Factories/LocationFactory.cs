using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class LocationFactory
    {
        public static Location Create(SystemMessages msgs, DatabaseConnection dc)
        {
            var location = new Location()
            {
                Name = "My Location",
                Address1 = "1234 N Whatever Street",
                Address2 = "Suite 2",
                City = "Indianapolis",
                State = "IN",
                PostalCode = "12345",
                Country = "US"
            };
            msgs.AddRange(location.Save(dc));
            return location;
        }
        public static Location Create(Account hostAccount, SystemMessages msgs, DatabaseConnection dc)
        {
            var location = new Location()
            {
                HostAccountId = hostAccount.ID,
                Name = "My Location",
                Address1 = "1234 N Whatever Street",
                Address2 = "Suite 2",
                City = "Indianapolis",
                State = "IN",
                PostalCode = "12345",
                Country = "US"
            };
            msgs.AddRange(location.Save(dc));
            return location;
        }
    }
}
