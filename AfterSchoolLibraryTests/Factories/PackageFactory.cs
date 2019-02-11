using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class PackageFactory
    {
        public static Package Create(SystemMessages msgs, DatabaseConnection dc)
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
        public static Package Create(decimal pricePerMonth, decimal pricePerYear, SystemMessages msgs, DatabaseConnection dc)
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
        public static Package CreateWithStripe(SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new Package()
            {
                Name = "Package Test",
                IsActive = true,
                Price = 0.ToDecimal(),
                PricePerYear = 0.ToDecimal()
            };
            msgs.AddRange(obj.SaveWithStripe(dc: dc));
            return obj;
        }
    }
}
