using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchool;

namespace AfterSchool
{
    public class DBConfig
    {
        public static void RegisterDatabase()
        {
            BasicLibrary.Utilities.ConnectionString = AfterSchoolLibrary.Helpers.Utilities.ConnectionString;
        }
    }
}
