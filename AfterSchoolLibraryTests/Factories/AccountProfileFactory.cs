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
    public class AccountProfileFactory
    {
        public static AccountProfile Create(Account account, SystemMessages msgs, DatabaseConnection dc)
        {
            var profile = new AccountProfile()
            {
                AccountId = account.ID.Value,
                CreateDate = DateTime.UtcNow,
                Title = account.Name,
            };

            msgs.AddRange(profile.Save(dc));

            return profile;
        }
    }
}
