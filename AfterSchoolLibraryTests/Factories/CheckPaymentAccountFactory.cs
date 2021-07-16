using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using System;

namespace AfterSchoolLibraryTests.Factories
{
    public class CheckPaymentAccountFactory
    {
        public static CheckPaymentAccount Create(Account account, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new CheckPaymentAccount()
            {
                AccountId = account.ID,
                IsVerified = true,
                LegalEntityTypeId = LegalEntityType.Individual,
                CreateDate = DateTime.Now,
                CreatedBy = 1,
                LastModifiedDate = DateTime.Now,
                LastModifiedBy = 1,
                Address1 = "Test Address 1",
                Address2 = "Test Address 2",
                City = "Test City",
                State = "Test State",
                PostalCode = "Test PostalCode",
                Country = "US",
                Name = "Test Name",
                Description = "Test Description"                
            };

            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
