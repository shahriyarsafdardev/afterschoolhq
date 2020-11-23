using System;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Billing
{
    public class Platform
    {        

        public static DateRange GetCurrentSchoolYear()
        {
            var startYear = DateTime.UtcNow.Year;

            if (DateTime.UtcNow.Month <= 6)
                startYear = DateTime.UtcNow.Year - 1;

            var startDate = $"07/01/{startYear}".ToDateTime();
            var endDate = startDate.AddYears(1).AddSeconds(-1);

            return new DateRange(startDate, endDate);
        }
    }
}
