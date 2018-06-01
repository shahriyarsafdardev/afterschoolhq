using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AfterSchool
{
    public class AppConfig
    {
        public static void Init()
        {
            RollbarSharp.Configuration.DefaultEnvironment = AfterSchoolLibrary.Utilities.GetRollbarEnvironment();

            AfterSchoolLibrary.Utilities.InitializeApp();
        }
    }
}
