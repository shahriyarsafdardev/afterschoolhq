using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Collections;
using BasicLibrary;
using AfterSchoolLibrary;

namespace AfterSchool.Models
{
    public class Utilities
    {
        public static bool IsAfterSchool()
        {
            if (!SessionVariables.IsAfterSchool.HasValue)
            {
                SessionVariables.IsAfterSchool = true;
            }

            return SessionVariables.IsAfterSchool.Value;
        }

        public static void DetectAfterSchool()
        {
            if (!SessionVariables.IsAfterSchool.HasValue)
            {
                IsAfterSchool();
            }
        }

        public static void SetAfterSchool(bool isAfterSchool)
        {
            SessionVariables.IsAfterSchool = true;
        }
    }
}
