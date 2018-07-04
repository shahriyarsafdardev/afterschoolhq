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
    public class ProductFactory
    {
        public static Product Create(Account account, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Product() { AccountId = account.ID, Name = "Product 1", Cost = 13.50.ToDecimal() };
            msgs.AddRange(obj.Save(dc));
            return obj;
        }
    }
}
