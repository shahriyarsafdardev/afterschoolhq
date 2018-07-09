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
    public class PackageFactory
    {
        public static Package Create(SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new Package()
            {
                Name = "Package Test",
                IsActive = true,
                Price = 0.ToDecimal(),
                PricePerYear = 0.ToDecimal()
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static Package Create(decimal pricePerMonth, decimal pricePerYear, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var obj = new Package()
            {
                Name = "Package Test",
                IsActive = true,
                Price = pricePerMonth,
                PricePerYear = pricePerYear
            };
            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}
