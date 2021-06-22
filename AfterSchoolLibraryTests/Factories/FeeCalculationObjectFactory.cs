using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using System;
using AfterSchoolLibrary.Services;
using System.Collections.Generic;

namespace AfterSchoolLibraryTests.Factories
{
    public class FeeCalculationObjectFactory
    {
        public static FeeCalculationObject Create(Account billingAccount, SystemMessages msgs, DatabaseConnection dc)
        {
            var cost = 100;
            return new FeeCalculationObject(cost, billingAccount, msgs, dc);
        }

       public static FeeCalculationObject Create(Account billingAccount, Activity activity, bool isConvenienceFeeTakenOutOfCost, SystemMessages msgs, DatabaseConnection dc)
        {
            var cost = 100;
            return new FeeCalculationObject(cost, billingAccount, activity, isConvenienceFeeTakenOutOfCost, msgs, dc);
        }
    }
}
