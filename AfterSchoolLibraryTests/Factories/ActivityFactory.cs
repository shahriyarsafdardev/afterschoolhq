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
    public class ActivityFactory
    {
        public static Activity Create(Account account, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new Activity() { AccountId = account.ID, SeatsAvailable = 4 };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
        public static Activity Create(Account account, int seatsAvailable, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new Activity() { AccountId = account.ID, SeatsAvailable = seatsAvailable };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
