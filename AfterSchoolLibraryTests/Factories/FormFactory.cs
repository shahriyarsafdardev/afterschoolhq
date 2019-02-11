using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class FormFactory
    {
        public static Form Create(Account account, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Form() { AccountId = account.ID };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
