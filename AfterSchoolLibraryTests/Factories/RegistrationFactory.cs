using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class RegistrationFactory
    {
        public static Registration Create(Account account, Registrant registrant, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Registration() { AccountId = account.ID, RegistrantId = registrant.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Registration Create(Account account, Registrant registrant, RegistrationCompletionStatus completionStatusId, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Registration() { AccountId = account.ID, RegistrantId = registrant.ID, CompletionStatusId = completionStatusId };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static Registration Create(Account account, Registrant registrant, int couponId, bool isDeleted, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Registration() { AccountId = account.ID, RegistrantId = registrant.ID, CouponId = couponId, IsDeleted = isDeleted };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
